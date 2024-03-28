// Copyright (c) 2017-2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using DigitaleBriefwahl.ExceptionHandling;
using IniParser.Model;

namespace DigitaleBriefwahl.Model
{
	public class WeightedElectionModel: ElectionModel
	{
		public WeightedElectionModel(string name, IniData data) : base(name, data)
		{
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
				for (int i = 0; i < Votes; i++)
				{
					votes.Add(Nominees[i]);
				}

				return votes;
			}
		}

		protected override Dictionary<string, ElectionResult> ReadVotesFromBallotInternal
			(StreamReader	stream)
		{
			var skipLine = stream.ReadLine();
			var instructions = new Regex("(\\d Stimmen; Wahl der Reihenfolge nach mit 1.-\\d. kennzeichnen)");
			if (string.IsNullOrEmpty(skipLine) || !instructions.IsMatch(skipLine))
			{
				Logger.Error($"Missing line '({Votes} Stimmen; Wahl der Reihenfolge nach mit 1.-{Votes}. kennzeichnen)'. Got {skipLine}");
				Invalid++;
				return new Dictionary<string, ElectionResult>();
			}

			var votes = new Dictionary<string, ElectionResult>();
			foreach (var nominee in Nominees)
			{
				votes[nominee] = new WeightedElectionResult();
			}

			var nomineesSeen = new List<string>();
			var rankSeen = new List<int>();
			var invalid = false;

			for (var line = stream.ReadLine(); !string.IsNullOrEmpty(line); line = stream.ReadLine())
			{
				// 1. Mickey Mouse
				ElectionResult res = null;
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

				if (res is WeightedElectionResult result)
				{
					result.Points += Votes - rank + 1;
				}
			}

			if (invalid)
				Invalid++;

			return votes;
		}
	}
}