// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using System.Text;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Tally
{
	public class ReadBallot
	{
		private readonly Configuration _configuration;
		private readonly string _content;

		public ReadBallot(Configuration configuration, string ballotFileName)
		{
			_configuration = configuration;
			_content = File.ReadAllText(ballotFileName);
		}

		public (bool, Dictionary<ElectionModel, BallotResult>) AddBallot(
			Dictionary<ElectionModel, Dictionary<string, CandidateResult>> talliedResults)
		{
			using var ballotFile = new StringReader(_content);
			var title = ballotFile.ReadLine();     // The election
			var separator = ballotFile.ReadLine(); // ============
			if (string.IsNullOrWhiteSpace(separator) || !separator.StartsWith("="))
			{
				return (false, null);
			}
			SkipEmptyLines(ballotFile);
			var ballotInvalid = false;
			var ballotResults = new Dictionary<ElectionModel, BallotResult>();
			foreach (var election in _configuration.Elections)
			{
				var electionTitle = ballotFile.ReadLine(); // Election
				separator = ballotFile.ReadLine();         // --------
				if (string.IsNullOrWhiteSpace(separator) || !separator.StartsWith("--") || electionTitle != election.Name)
				{
					return (false, null);
				}

				var prevInvalid = election.Invalid;
				talliedResults[election] = election.ReadVotesFromBallot(ballotFile, talliedResults.TryGetValue(election, out var result) ? result: null);
				ballotResults[election] = election.BallotResults[election.BallotResults.Count - 1];
				ballotInvalid |= election.Invalid > prevInvalid;
			}

			return (ballotInvalid, ballotResults);
		}

		private void SkipEmptyLines(TextReader stream)
		{
			int peek;
			while ((peek = stream.Peek()) > -1)
			{
				if ((char)peek != '\r' && (char)peek != '\n')
					return;

				stream.ReadLine();
			}
		}

		public string PrintBallot(int id)
		{
			var bldr = new StringBuilder();
			bldr.AppendLine();
			bldr.AppendLine();
			bldr.AppendLine();
			bldr.AppendLine();
			bldr.AppendLine($"          D{id}");
			bldr.AppendLine();

			var reader = new StringReader(_content);
			for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
			{
				if (string.IsNullOrEmpty(line))
				{
					bldr.AppendLine();
				}
				else
				{
					bldr.Append("          ");
					bldr.AppendLine(line);
				}
			}

			bldr.Append("\f");

			return bldr.ToString();
		}
	}
}