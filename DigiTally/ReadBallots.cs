// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using System.Text;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Tally
{
	public class ReadBallots
	{
		private Configuration _configuration;
		private Dictionary<ElectionModel, Dictionary<string, CandidateResult>> _results;

		public ReadBallots(string configFileName)
		{
			_results = new Dictionary<ElectionModel, Dictionary<string, CandidateResult>>();
			_configuration = Configuration.Configure(configFileName);
		}

		public bool AddBallot(string ballotFileName)
		{
			using var ballotFile = new StreamReader(ballotFileName);
			var title = ballotFile.ReadLine();     // The election
			var separator = ballotFile.ReadLine(); // ============
			if (string.IsNullOrWhiteSpace(separator) || !separator.StartsWith("="))
			{
				return false;
			}
			SkipEmptyLines(ballotFile);
			var ballotIsPartiallyInvalid = false;
			foreach (var election in _configuration.Elections)
			{
				var electionTitle = ballotFile.ReadLine(); // Election
				separator = ballotFile.ReadLine();         // --------
				if (string.IsNullOrWhiteSpace(separator) || !separator.StartsWith("--") || electionTitle != election.Name)
				{
					return false;
				}

				var prevInvalid = election.Invalid;
				_results[election] = election.ReadVotesFromBallot(ballotFile, _results.TryGetValue(election, out var result) ? result: null);
				ballotIsPartiallyInvalid |= election.Invalid > prevInvalid;
			}

			NumberOfBallots++;

			if (ballotIsPartiallyInvalid)
				NumberOfInvalidBallots++;

			return true;
		}

		private void SkipEmptyLines(StreamReader stream)
		{
			int peek;
			while ((peek = stream.Peek()) > -1)
			{
				if ((char)peek != '\r' && (char)peek != '\n')
					return;

				stream.ReadLine();
			}
		}

		public int NumberOfBallots { get; private set; }
		public int NumberOfInvalidBallots { get; private set; }
		public Dictionary<ElectionModel, Dictionary<string, CandidateResult>> Results => _results;

		public string GetResultString()
		{
			var strBuilder = new StringBuilder();
			foreach (var election in _results)
			{
				strBuilder.AppendLine($"{election.Key.Name}");
				strBuilder.AppendLine(new string('-', election.Key.Name.Length));
				strBuilder.AppendLine(election.Key.GetResultString(election.Value));
			}
			return strBuilder.ToString();
		}
	}
}