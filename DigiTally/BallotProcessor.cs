// Copyright (c) 2026 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DigitaleBriefwahl.Model;
using SIL.Providers;

namespace DigitaleBriefwahl.Tally
{
	public class BallotProcessor
	{
		private DigiTallyOptions Options { get; }
		private readonly Dictionary<ElectionModel, Dictionary<string, CandidateResult>> _talliedResults;
		private readonly List<Dictionary<ElectionModel, BallotResult>> _ballotResults;
		private Configuration _configuration;
		private readonly StringBuilder _printedBallots;

		public int NumberOfBallots { get; private set; }
		public int NumberOfInvalidBallots { get; private set; }
		public string CsvFile { get; private set; }
		public Dictionary<ElectionModel, Dictionary<string, CandidateResult>> TalliedResults => _talliedResults;
		public IReadOnlyList<Dictionary<ElectionModel, BallotResult>> BallotResults => _ballotResults;


		public BallotProcessor(DigiTallyOptions options)
		{
			Options = options;
			_talliedResults = new Dictionary<ElectionModel, Dictionary<string, CandidateResult>>();
			_ballotResults = new List<Dictionary<ElectionModel, BallotResult>>();
			_printedBallots = new StringBuilder();
		}

		public string GetElectionResults()
		{
			var strBuilder = new StringBuilder();
			ProcessBallots();
			strBuilder.Append(_printedBallots);
			if (Options.PrintBallots)
			{
				strBuilder.AppendLine();
				strBuilder.AppendLine();
				strBuilder.AppendLine();
				strBuilder.AppendLine();
			}

			var reader = new StringReader(TallyBallots());
			for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
			{
				if (!string.IsNullOrWhiteSpace(line) && Options.PrintBallots)
					strBuilder.Append("          ");
				strBuilder.AppendLine(line);
			}
			return strBuilder.ToString();
		}

		public void ProcessBallots()
		{
			var configName = Options.ConfigFile ?? Configuration.ConfigName;
			_configuration = Configuration.Configure(configName);

			var directory = new DirectoryInfo(Options.BallotsDirectory);
			foreach (var file in directory.GetFiles().OrderBy(f => f.CreationTimeUtc))
			{
				NumberOfBallots++;
				var readBallot = new ReadBallot(_configuration, file.FullName);
				if (Options.PrintBallots)
				{
					_printedBallots.AppendLine(readBallot.PrintBallot(NumberOfBallots));
				}

				var (invalid, thisResult) = readBallot.AddBallot(_talliedResults);

				if (invalid)
					NumberOfInvalidBallots++;

				_ballotResults.Add(thisResult);
			}

			if (Options.CreateCsv)
			{
				var csvExporter = new CsvExporter(Options);
				CsvFile = csvExporter.CreateCsvForBallots(_ballotResults);
			}
		}

		public string GetResultString()
		{
			var strBuilder = new StringBuilder();
			foreach (var election in _talliedResults)
			{
				strBuilder.AppendLine($"{election.Key.Name}");
				strBuilder.AppendLine(new string('-', election.Key.Name.Length));
				strBuilder.AppendLine(election.Key.GetResultString(election.Value));
			}

			strBuilder.AppendLine($"(DigiTally version {GitVersionInformation.SemVer}; " +
			                      $"report executed {DateTimeProvider.Current.Now:yyyy-MM-dd HH:mm})");
			return strBuilder.ToString();
		}

		public string TallyBallots()
		{
			var bldr = new StringBuilder();
			bldr.AppendLine(_configuration.Title);
			bldr.AppendLine(new string('=', _configuration.Title.Length));
			bldr.AppendLine();
			bldr.AppendLine(
				$"Total of {NumberOfBallots} ballots, thereof {NumberOfInvalidBallots} at least partially invalid.");
			bldr.AppendLine();
			bldr.Append(GetResultString());
			return bldr.ToString();
		}
	}
}