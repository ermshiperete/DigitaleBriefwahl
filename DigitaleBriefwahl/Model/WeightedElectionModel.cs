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

		public override Dictionary<string, ElectionResult> ReadVotesFromBallot(StreamReader stream, Dictionary<string, ElectionResult> votes)
		{
			var skipLine = stream.ReadLine();
			var instructions =
				new Regex("(\\d Stimmen; Wahl der Reihenfolge nach mit 1.-\\d. kennzeichnen)");
			if (!instructions.IsMatch(skipLine))
			{
				Debug.WriteLine($"Missing line '({Votes} Stimmen; Wahl der Reihenfolge nach mit 1.-{Votes}. kennzeichnen)'. Got {skipLine}");
				return votes;
			}

			votes ??= new Dictionary<string, ElectionResult>();

			for (var line = stream.ReadLine(); !string.IsNullOrEmpty(line); line = stream.ReadLine())
			{
				// 1. Mickey Mouse
				var regex = new Regex("(([0-9]+).|  ) (.+)");
				if (!regex.IsMatch(line))
				{
					Debug.WriteLine($"Can't interpret {line}");
					continue;
				}

				var match = regex.Match(line);
				var name = match.Groups[3].Value;
				if (!votes.TryGetValue(name, out var res))
				{
					res = new WeightedElectionResult();
					votes[name] = res;
				}

				var result = res as WeightedElectionResult;
				if (string.IsNullOrWhiteSpace(match.Groups[1].Value))
					continue;

				if (!Int32.TryParse(match.Groups[2].Value, out var rank))
				{
					Debug.WriteLine($"Invalid rank {match.Groups[2].Value} in line {line}");
					result.Invalid++;
					continue;
				}

				result.Points += Votes - rank + 1;
			}

			return votes;
		}
	}
}