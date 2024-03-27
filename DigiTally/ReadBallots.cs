using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Tally
{
	public class ReadBallots
	{
		private Configuration                            _configuration;
		private Dictionary<ElectionModel, Dictionary<string, ElectionResult>> _results;

		public ReadBallots(string configFileName)
		{
			_results = new Dictionary<ElectionModel, Dictionary<string, ElectionResult>>();
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
			foreach (var election in _configuration.Elections)
			{
				var electionTitle = ballotFile.ReadLine(); // Election
				separator = ballotFile.ReadLine();         // --------
				if (string.IsNullOrWhiteSpace(separator) || !separator.StartsWith("--") || electionTitle != election.Name)
				{
					return false;
				}

				_results[election] = election.ReadVotesFromBallot(ballotFile, _results.TryGetValue(election, out var result) ? result: null);
			}

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

		public int NumberOfInvalidBallots => _results.Count(r => r.Key.Invalid > 0);
		public Dictionary<ElectionModel, Dictionary<string, ElectionResult>> Results => _results;
	}
}