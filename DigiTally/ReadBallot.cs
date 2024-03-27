using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Tally
{
	public class ReadBallot
	{
		private Configuration                            _configuration;
		private Dictionary<ElectionModel, Dictionary<string, ElectionResult>> _results;

		public ReadBallot(string configFileName)
		{
			_results = new Dictionary<ElectionModel, Dictionary<string, ElectionResult>>();
			_configuration = Configuration.Configure(configFileName);
			IsReadable = true;
		}

		public void AddBallot(string ballotFileName)
		{
			using var ballotFile = new StreamReader(ballotFileName);
			var title = ballotFile.ReadLine();     // The election
			var separator = ballotFile.ReadLine(); // ============
			if (!separator.StartsWith("="))
			{
				IsReadable = false;
				return;
			}
			SkipEmptyLines(ballotFile);
			foreach (var election in _configuration.Elections)
			{
				var electionTitle = ballotFile.ReadLine(); // Election
				separator = ballotFile.ReadLine();         // --------
				if (!separator.StartsWith("--") || electionTitle != election.Name)
				{
					IsReadable = false;
					return;
				}

				_results[election] = election.ReadVotesFromBallot(ballotFile, _results.TryGetValue(election, out var result) ? result: null);
			}
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

		public bool IsReadable { get; private set; }

		public Dictionary<ElectionModel, Dictionary<string, ElectionResult>> Results => _results;
	}
}