// Copyright (c) 2026 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Tally
{
	public class CsvExporter
	{
		private DigiTallyOptions Options { get; }
		private readonly Configuration _configuration;

		public CsvExporter(DigiTallyOptions options)
		{
			Options = options;
			var configName = Options.ConfigFile ?? Configuration.ConfigName;
			_configuration = Configuration.Configure(configName);
		}

		public string CreateCsvForBallots(IReadOnlyList<Dictionary<ElectionModel, BallotResult>> ballotResults)
		{
			if (!CanCreateCsv(_configuration.Elections))
				return null;

			var rows = new List<List<string>> { CreateCsvHeader(_configuration.Elections) };
			var ballotNumber = 1;
			foreach (var ballotResult in ballotResults)
			{
				if (ballotResult != null)
					rows.Add(CreateCsvRow(ballotResult, _configuration.Elections, ballotNumber));
				ballotNumber++;
			}

			string csvFileName;
			if (string.IsNullOrEmpty(Options.CsvFile))
				csvFileName = $"{_configuration.FileTitle}.csv";
			else
				csvFileName = Options.CsvFile;
			File.WriteAllLines(csvFileName, rows.Select(row => string.Join(";", row) + ";"), Encoding.UTF8);
			return csvFileName;
		}

		private bool CanCreateCsv(IReadOnlyList<ElectionModel> elections)
		{
			if (elections.Count < 1 || elections.Count > 3)
				return false;

			var validTitles = new HashSet<string>(CsvElectionTitles);
			foreach (var election in elections)
			{
				if (!validTitles.Remove(election.Name))
					return false;
			}

			return true;
		}

		private List<string> CreateCsvHeader(IReadOnlyList<ElectionModel> elections)
		{
			var header = new List<string> { "Nummer", string.Empty };
			AddWeightedHeader(header, GetElection(elections, EhrenamtlicherVorstand), 7, "Name Kandidat");
			header.Add("Ehrenamtl. Vorstand inkomplett");
			header.Add(string.Empty);
			AddWeightedHeader(header, GetElection(elections, Fachvorstand), 15, "Name Kandidat");
			header.Add("Fachvorstand inkomplett");
			header.Add(string.Empty);
			header.Add("Ja");
			header.Add("Nein");
			header.Add("Enth.");
			return header;
		}

		private void AddWeightedHeader(List<string> header, ElectionModel election, int candidateCount, string placeholderPrefix)
		{
			for (var i = 0; i < candidateCount; i++)
			{
				if (election != null && i < election.Nominees.Count)
					header.Add(election.Nominees[i]);
				else
					header.Add($"{placeholderPrefix} {i + 1}");
			}
		}

		private List<string> CreateCsvRow(Dictionary<ElectionModel, BallotResult> ballotResults, IReadOnlyList<ElectionModel> elections, int ballotNumber)
		{
			var row = new List<string> { ballotNumber.ToString(), string.Empty };
			AddWeightedRow(row, ballotResults, GetElection(elections, EhrenamtlicherVorstand), 7);
			row.Add(GetIncompleteValue(ballotResults, EhrenamtlicherVorstand));
			row.Add(string.Empty);
			AddWeightedRow(row, ballotResults, GetElection(elections, Fachvorstand), 15);
			row.Add(GetIncompleteValue(ballotResults, Fachvorstand));
			row.Add(string.Empty);
			AddYesNoRow(row, ballotResults, GetElection(elections, Vorstandsvorsitzender));
			return row;
		}

		private void AddWeightedRow(List<string> row, Dictionary<ElectionModel, BallotResult> ballotResults, ElectionModel election, int candidateCount)
		{
			var ballotResult = GetBallotResult(ballotResults, election);
			for (var i = 0; i < candidateCount; i++)
			{
				var rank = ballotResult != null && i < ballotResult.WeightedRanks.Count
					? ballotResult.WeightedRanks[i]
					: 0;
				row.Add(rank > 0 ? rank.ToString() : string.Empty);
			}
		}

		private void AddYesNoRow(List<string> row, Dictionary<ElectionModel, BallotResult> ballotResults, ElectionModel election)
		{
			var ballotResult = GetBallotResult(ballotResults, election);
			var yes = string.Empty;
			var no = string.Empty;
			var abstention = string.Empty;
			if (ballotResult != null)
			{
				foreach (var vote in ballotResult.YesNoVotes)
				{
					switch (vote)
					{
						case YesNoElectionModel.Yes:
							yes = "1";
							break;
						case YesNoElectionModel.No:
							no = "1";
							break;
						case YesNoElectionModel.Abstention:
							abstention = "1";
							break;
					}
				}
			}

			row.Add(yes);
			row.Add(no);
			row.Add(abstention);
		}

		private string GetIncompleteValue(Dictionary<ElectionModel, BallotResult> ballotResults, string title)
		{
			var ballotResult = GetBallotResult(ballotResults, title);
			return ballotResult == null || !(ballotResult.Invalid || ballotResult.Incomplete) ?
				string.Empty : "1";
		}

		private ElectionModel GetElection(IReadOnlyList<ElectionModel> elections, string title)
		{
			return elections.FirstOrDefault(election => election.Name == title);
		}

		private BallotResult GetBallotResult(Dictionary<ElectionModel, BallotResult> ballotResults, ElectionModel election)
		{
			return election != null && ballotResults.TryGetValue(election, out var ballotResult) ? ballotResult : null;
		}

		private BallotResult GetBallotResult(Dictionary<ElectionModel, BallotResult> ballotResults, string title)
		{
			return ballotResults.FirstOrDefault(kv => kv.Key.Name == title).Value;
		}

		private const string EhrenamtlicherVorstand = "Ehrenamtlicher Vorstand";
		private const string Fachvorstand = "Fachvorstand";
		private const string Vorstandsvorsitzender = "Vorstandsvorsitzender";
		private readonly string[] CsvElectionTitles =
		{
			EhrenamtlicherVorstand,
			Fachvorstand,
			Vorstandsvorsitzender
		};
	}
}