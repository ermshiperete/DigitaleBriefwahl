// Copyright (c) 2017-2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DigitaleBriefwahl.ExceptionHandling;
using IniParser.Model;

namespace DigitaleBriefwahl.Model
{
	public class YesNoElectionModel: ElectionModel
	{
		private class NomineeOrderComparer : IComparer<string>
		{
			public NomineeOrderComparer(List<string> nominees)
			{
				Nominees = nominees;
			}
			private List<string> Nominees { get; }

			public int Compare(string x, string y)
			{
				return Nominees.IndexOf(x).CompareTo(Nominees.IndexOf(y));
			}
		}

		public YesNoElectionModel(string name, IniData data) : base(name, data)
		{
		}

		public const string Yes = "J";
		public const string No = "N";
		public const string Abstention = "E";

		public override string GetResult(List<string> votes, bool writeEmptyBallot)
		{
			if (votes.Count != Nominees.Count)
			{
				throw new ArgumentException("Number of votes is not equal to number of nominees",
					nameof(votes));
			}

			var bldr = new StringBuilder();
			bldr.AppendLine(Name);
			bldr.Append('-', Name.Length);
			bldr.AppendLine();
			bldr.AppendLine("(J=Ja, E=Enthaltung, N=Nein)");

			var voteCount = 0;

			for (var i = 0; i < Nominees.Count; i++)
			{
				var vote = votes[i];
				if (vote.Length != 1)
				{
					throw new ArgumentException(
						"Invalid input string (expected exactly one character per vote)",
						nameof(votes));
				}

				switch (vote)
				{
					case No:
						break;
					case Yes:
						voteCount++;
						break;
					case Abstention:
						if (!writeEmptyBallot)
							voteCount++;
						break;
					default:
					{
						throw new ArgumentException($"Invalid vote character '{vote}'",
							nameof(votes));
					}
				}

				if (voteCount > Votes)
				{
					throw new ArgumentException($"Invalid number of votes. Allowed are {Votes}",
						nameof(votes));
				}

				bldr.AppendFormat("{0}. [{1}] {2}\n", i + 1, vote, Nominees[i]);
			}
			return NormalizeLineEndings(bldr);
		}

		public override List<string> EmptyVotes
		{
			get
			{
				var votes = new List<string>();
				for (int i = 0; i < Votes; i++)
					votes.Add(Abstention);
				return votes;
			}
		}

		protected override Dictionary<string, CandidateResult> ReadVotesFromBallotInternal(StreamReader stream)
		{
			var skipLine = stream.ReadLine();
			if (skipLine != "(J=Ja, E=Enthaltung, N=Nein)")
			{
				Logger.Error($"Missing line '(J=Ja, E=Enthaltung, N=Nein)'. Got {skipLine}");
				return null;
			}

			var nomineesSeen = new List<string>();
			var votes = new Dictionary<string, CandidateResult>();
			var invalid = false;

			for (var line = stream.ReadLine(); !string.IsNullOrEmpty(line); line = stream.ReadLine())
			{
				// 1. [J] Mickey Mouse
				var regex = new Regex("[0-9]+. \\[(J|E|N|( )*)\\] (.+)");
				if (!regex.IsMatch(line))
				{
					Logger.Error($"Can't interpret {line}");
					invalid = true;
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
				if (!votes.TryGetValue(name, out var res))
				{
					res = new YesNoCandidateResult();
					votes[name] = res;
				}

				var result = res as YesNoCandidateResult;
				switch (match.Groups[1].Value)
				{
					case Yes:
						result.Yes++;
						break;
					case No:
						result.No++;
						break;
					case Abstention:
					default:
						result.Abstention++;
						break;
				}
			}

			if (invalid)
			{
				Invalid++;
				return null;
			}
			return votes;
		}

		public override string GetResultString(Dictionary<string, CandidateResult> results)
		{
			var bldr = new StringBuilder();
			var comparer = new NomineeOrderComparer(Nominees);

			var candidates = results.Keys.OrderBy(n => n, comparer);
			foreach (var candidate in candidates)
			{
				var ynResult = (YesNoCandidateResult)results[candidate];
				bldr.AppendLine($"{candidate}: {ynResult.Yes} J, {ynResult.No} N, {ynResult.Abstention} E");
			}

			bldr.AppendLine(base.GetResultString(results));
			return bldr.ToString();
		}
	}
}