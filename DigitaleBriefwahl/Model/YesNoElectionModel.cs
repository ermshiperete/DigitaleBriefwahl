// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.Text;
using IniParser.Model;

namespace DigitaleBriefwahl.Model
{
	public class YesNoElectionModel: ElectionModel
	{
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
	}
}