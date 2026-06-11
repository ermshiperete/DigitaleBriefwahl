// Copyright (c) 2026 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class CsvExporterTests
	{
		private string _configFileName;
		private string _ballotDirectoryName;

		[SetUp]
		public void Setup()
		{
			_ballotDirectoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_ballotDirectoryName);
			_configFileName = Path.GetTempFileName();
		}

		[TearDown]
		public void Teardown()
		{
			File.Delete(_configFileName);
			Directory.Delete(_ballotDirectoryName, true);
		}

		[Test]
		public void CreateCsvForBallots_ReturnsNull_WhenTooManyElections()
		{
			// Setup
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Ehrenamtlicher Vorstand
Wahl2=Fachvorstand
Wahl3=Vorstandsvorsitzender
Wahl4=Election1
Email=election@example.com
PublicKey=12345678.asc

[Ehrenamtlicher Vorstand]
Text=Some description
Typ=Weighted
Stimmen=1
Kandidat1=Extra

[Fachvorstand]
Text=Some description
Typ=Weighted
Stimmen=1
Kandidat1=Extra

[Vorstandsvorsitzender]
Text=Some description
Typ=Weighted
Stimmen=1
Kandidat1=Extra

[Election1]
Text=Some description
Typ=Weighted
Stimmen=1
Kandidat1=Extra
");
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = true
			};
			var sut = new CsvExporter(options);

			// Execute
			var csvFile = sut.CreateCsvForBallots(null);

			// Verify
			Assert.That(csvFile, Is.Null);
		}

		[Test]
		public void CreateCsvForBallots_HappyPath()
		{
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Fachvorstand
Wahl2=Ehrenamtlicher Vorstand
Email=election@example.com
PublicKey=12345678.asc

[Fachvorstand]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck

[Ehrenamtlicher Vorstand]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=One
Kandidat2=Two
Kandidat3=Three
Kandidat4=Four
");
			// Write ballots
			var ballotFileName = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Fachvorstand\r\n" +
			                                   "------------\r\n" +
			                                   "(3 Stimmen; Wahl der Reihenfolge nach mit 1.-3. kennzeichnen)\r\n" +
			                                   "2. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "1. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Ehrenamtlicher Vorstand\r\n" +
			                                   "-----------------------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "1. One\r\n" +
			                                   "2. Two\r\n" +
			                                   "   Three\r\n" +
			                                   "   Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = false
			};

			var ballotProcessor = new BallotProcessor(options);
			ballotProcessor.ProcessBallots();

			options.CreateCsv = true;
			var sut = new CsvExporter(options);

			// Execute
			var csvFile = sut.CreateCsvForBallots(ballotProcessor.BallotResults);

			// Verify
			Assert.That(csvFile, Is.EqualTo("The_election.csv"));
			Assert.That(File.Exists(csvFile), Is.True);
			var content = File.ReadAllText(csvFile);
			Assert.That(content, Is.EqualTo(@"Nummer;;One;Two;Three;Four;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Ehrenamtl. Vorstand inkomplett;;Mickey Mouse;Donald Duck;Dagobert Duck;Daisy Duck;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Name Kandidat 8;Name Kandidat 9;Name Kandidat 10;Name Kandidat 11;Name Kandidat 12;Name Kandidat 13;Name Kandidat 14;Name Kandidat 15;Fachvorstand inkomplett;;Ja;Nein;Enth.;
1;;1;2;;;;;;;;2;;1;;;;;;;;;;;;;1;;;;;
"));
		}

		[Test]
		public void CreateCsvForBallots_FilenameSpecified()
		{
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Fachvorstand
Wahl2=Ehrenamtlicher Vorstand
Email=election@example.com
PublicKey=12345678.asc

[Fachvorstand]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck

[Ehrenamtlicher Vorstand]
Text=Some description
Typ=YesNo
Stimmen=2
Kandidat1=One
Kandidat2=Two
Kandidat3=Three
Kandidat4=Four
");
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = true,
				CsvFile = Path.GetTempFileName()
			};

			var sut = new CsvExporter(options);

			// Execute
			var csvFile = sut.CreateCsvForBallots(new List<Dictionary<ElectionModel, BallotResult>>());

			// Verify
			Assert.That(csvFile, Is.EqualTo(options.CsvFile));
			Assert.That(File.Exists(csvFile), Is.True);
			var content = File.ReadAllText(csvFile);
			Assert.That(content, Is.EqualTo(@"Nummer;;One;Two;Three;Four;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Ehrenamtl. Vorstand inkomplett;;Mickey Mouse;Donald Duck;Dagobert Duck;Daisy Duck;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Name Kandidat 8;Name Kandidat 9;Name Kandidat 10;Name Kandidat 11;Name Kandidat 12;Name Kandidat 13;Name Kandidat 14;Name Kandidat 15;Fachvorstand inkomplett;;Ja;Nein;Enth.;
"));
		}

		[Test]
		public void CreateCsvForBallots_IgnoreInvalidBallotFile()
		{
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Fachvorstand
Wahl2=Ehrenamtlicher Vorstand
Email=election@example.com
PublicKey=12345678.asc

[Fachvorstand]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck

[Ehrenamtlicher Vorstand]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=One
Kandidat2=Two
Kandidat3=Three
Kandidat4=Four
");
			// Write ballots
			var ballotFileName = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "a;b;c");
			var options = new DigiTallyOptions
			{
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = false
			};

			var ballotProcessor = new BallotProcessor(options);
			ballotProcessor.ProcessBallots();

			options.CreateCsv = true;
			var sut = new CsvExporter(options);

			// Execute
			var csvFile = sut.CreateCsvForBallots(ballotProcessor.BallotResults);

			// Verify
			Assert.That(csvFile, Is.EqualTo("The_election.csv"));
			Assert.That(File.Exists(csvFile), Is.True);
			var content = File.ReadAllText(csvFile);
			Assert.That(content, Is.EqualTo("Nummer;;One;Two;Three;Four;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Ehrenamtl. Vorstand inkomplett;;Mickey Mouse;Donald Duck;Dagobert Duck;Daisy Duck;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Name Kandidat 8;Name Kandidat 9;Name Kandidat 10;Name Kandidat 11;Name Kandidat 12;Name Kandidat 13;Name Kandidat 14;Name Kandidat 15;Fachvorstand inkomplett;;Ja;Nein;Enth.;\n"));
		}
	}
}
