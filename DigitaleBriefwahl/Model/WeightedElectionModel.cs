// Copyright (c) 2017-2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DigitaleBriefwahl.ExceptionHandling;
using IniParser.Model;

namespace DigitaleBriefwahl.Model
{
	public class WeightedElectionModel: ElectionModel
	{
		private bool MissingOk { get; }
		public WeightedElectionModel(string name, IniData data) : base(name, data)
		{
			MissingOk = data[name].ContainsKey(Configuration.MissingOk) && data[name][Configuration.MissingOk] == "true";
		}

		public override string GetResult(List<string> electedNominees, bool writeEmptyBallot)
		{
			var bldr = new StringBuilder();
			bldr.AppendLine(Name);
			bldr.Append('-', Name.Length);
			bldr.AppendLine();
			bldr.AppendLine(Votes > 1
				? $"({Votes} Stimmen; Wahl der Reihenfolge nach mit 1.-{Votes}. kennzeichnen)"
				: $"({Votes} Stimme; Wahl mit 1. kennzeichnen)");

			foreach (var nominee in Nominees)
			{
				if (electedNominees.Contains(nominee) && !writeEmptyBallot)
				{
					for (var i = 0; i < Votes; i++)
					{
						if (electedNominees[i] != nominee)
							continue;

						bldr.AppendLine($"{i + 1}. {nominee}");
						break;
					}
				}
				else
				{
					bldr.AppendLine($"   {nominee}");
				}
			}

			return NormalizeLineEndings(bldr);
		}

		public override List<string> EmptyVotes
		{
			get
			{
				var votes = new List<string>();
				for (int i = 0; i < Votes && i < Nominees.Count; i++)
				{
					votes.Add(Nominees[i]);
				}

				return votes;
			}
		}

		protected override Dictionary<string, CandidateResult> ReadVotesFromBallotInternal(StreamReader stream)
		{
			var skipLine = stream.ReadLine();
			var instructions = new Regex("\\(\\d Stimme.+");
			if (string.IsNullOrEmpty(skipLine) || !instructions.IsMatch(skipLine))
			{
				Logger.Error($"Missing line '({Votes} Stimmen; Wahl der Reihenfolge nach mit 1.-{Votes}. kennzeichnen)'. Got {skipLine}");
				Invalid++;
				return null;
			}

			var votes = new Dictionary<string, CandidateResult>();
			foreach (var nominee in Nominees)
			{
				votes[nominee] = new WeightedCandidateResult();
			}

			var nomineesSeen = new List<string>();
			var rankSeen = new List<int>();
			var invalid = false;

			for (var line = stream.ReadLine(); !string.IsNullOrEmpty(line); line = stream.ReadLine())
			{
				// 1. Mickey Mouse
				CandidateResult res = null;
				var regex = new Regex("(([0-9]+).|  ) (.+)");
				if (!regex.IsMatch(line))
				{
					Logger.Error($"Can't interpret {line}");
					continue;
				}

				var match = regex.Match(line);
				var name = match.Groups[3].Value;
				if (nomineesSeen.Contains(name))
				{
					// we saw this name before - INVALID
					Logger.Error($"Double name {name}");
					invalid = true;
					continue;
				}

				nomineesSeen.Add(name);
				if (!votes.TryGetValue(name, out res))
				{
					// Name is not in the nominee list - INVALID
					Logger.Error($"{name} is not nominated.");
					invalid = true;
					continue;
				}

				if (string.IsNullOrWhiteSpace(match.Groups[1].Value))
					continue;

				if (!Int32.TryParse(match.Groups[2].Value, out var rank))
				{
					Logger.Error($"Invalid rank {match.Groups[2].Value} in line {line}");
					invalid = true;
					continue;
				}

				if (rankSeen.Contains(rank))
				{
					Logger.Error($"Double rank: {rank} ({name})");
					invalid = true;
					continue;
				}
				rankSeen.Add(rank);

				if (res is WeightedCandidateResult result)
				{
					result.Points += Votes - rank + 1;
				}
			}

			if (invalid)
			{
				Invalid++;
				return null;
			}

			return votes;
		}

		private class FindIndexPredicate
		{
			private string Name;
			public FindIndexPredicate(string name)
			{
				Name = name;
			}

			public bool Find(KeyValuePair<string, CandidateResult> kv)
			{
				return kv.Key == Name;
			}
		}

		private class RankComparer : IComparer<KeyValuePair<string, CandidateResult>>
		{
			public RankComparer(List<KeyValuePair<string, CandidateResult>> results)
			{
				_results = results;
			}

			private readonly List<KeyValuePair<string, CandidateResult>> _results;

			public int Compare(KeyValuePair<string, CandidateResult> x, KeyValuePair<string, CandidateResult> y)
			{
				var pointsX = ((WeightedCandidateResult)x.Value).Points;
				var pointsY = ((WeightedCandidateResult)y.Value).Points;
				// if the points are equal, we compare the names. But since we're sorting
				// the ranks descending we'll have to reverse the order of the names.
				return pointsX == pointsY ? string.Compare(x.Key, y.Key, StringComparison.Ordinal) * -1 : pointsX.CompareTo(pointsY);
			}
		}

		private static int GetRank(List<KeyValuePair<string, CandidateResult>> results, string
				candidate)
		{
			var index = results.FindIndex(new FindIndexPredicate(candidate).Find);
			if (index > 0 && ((WeightedCandidateResult)results[index].Value).Points == ((WeightedCandidateResult)results[index-1].Value).Points)
			{
				return index;
			}

			return index + 1;
		}

		public override string GetResultString(Dictionary<string, CandidateResult> results)
		{
			var bldr = new StringBuilder();
			var comparer = new RankComparer(results.ToList());
			var orderedResults = results.OrderByDescending(kv => kv, comparer).ToList();
			foreach (var kv in orderedResults)
			{
				var candidate = kv.Key;
				var weightedResult = (WeightedCandidateResult)kv.Value;

				var rank = GetRank(orderedResults, candidate);
				var placing = rank <= Votes ? $"{rank}." : "  ";
				bldr.AppendLine($"{placing} {candidate} ({weightedResult.Points} points)");
			}

			bldr.AppendLine(base.GetResultString(results));
			return bldr.ToString();
		}

		public override bool SkipNominee(string name, int iVote)
		{
			var vote = iVote + 1;
			if (!NomineeLimits.TryGetValue(name, out var limit))
				return false;

			return vote < limit.Item1 || vote > limit.Item2;
		}

		public override HashSet<int> GetInvalidVotes(List<string> electedNominees)
		{
			var invalid = new HashSet<int>();
			var lim = Math.Min(Votes, electedNominees.Count);
			for (int i = lim; i < Votes; i++)
			{
				if (!MissingOk)
				{
					// Missing votes
					invalid.Add(i);
				}

			}

			for (var i = 0; i < lim; i++)
			{
				var electedNominee = electedNominees[i]?.Trim();
				if (string.IsNullOrEmpty(electedNominee))
				{
					if (!MissingOk)
						invalid.Add(i);
					continue;
				}

				if (!Nominees.Contains(electedNominee) && electedNominee != Configuration.Abstention)
				{
					invalid.Add(i);
					continue;
				}

				for (var j = i + 1; j < lim; j++)
				{
					if (electedNominee != electedNominees[j] ||
						electedNominee == Configuration.Abstention)
					{
						continue;
					}

					invalid.Add(i);
					invalid.Add(j);
				}

				if (!SkipNominee(electedNominee, i))
					continue;

				invalid.Add(i);
			}
			return invalid;
		}
	}
}