// Copyright (c) 2026 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.Providers;
using SIL.TestUtilities.Providers;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class BallotProcessorTests
	{
		private string _configFileName;
		private string _ballotsPath;

		private void CreateSingleElectionWithOneCandidateConfig()
		{
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Election
Email=election@example.com
PublicKey=12345678.asc

[Election]
Text=Some description
Typ=Weighted
FehlendOk=true
Stimmen=2
Kandidat1=Mickey Mouse
");
		}

		private void CreateSingleWeightedElectionConfig()
		{
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Election
Email=election@example.com
PublicKey=12345678.asc

[Election]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck
");
		}

		private void CreateSingleYesNoElectionConfig()
		{
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Election
Email=election@example.com
PublicKey=12345678.asc

[Election]
Text=Some description
Typ=YesNo
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck
");
		}

		[SetUp]
		public void Setup()
		{
			_ballotsPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_ballotsPath);
			_configFileName = Path.GetTempFileName();
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
		}

		[TearDown]
		public void Teardown()
		{
			File.Delete(_configFileName);
			Directory.Delete(_ballotsPath, true);
		}

		[Test]
		public void ProcessBallots_Invalid_TwoNomineesOneVote()
		{
			// Setup
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Election
Email=election@example.com
PublicKey=12345678.asc

[Election]
Text=Some description
Typ=Weighted
FehlendOk=false
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Dagobert Duck
Kandidat3=Donald Duck
");
			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "2. Mickey Mouse\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "2. Mickey Mouse\r\n" +
			                                  "   Dagobert Duck\r\n" +
			                                  "1. Donald Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(2));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));
			var election1 = sut.TalliedResults.First();
			Assert.That(election1.Key.BallotsProcessed, Is.EqualTo(2));
			Assert.That(election1.Key.Invalid, Is.EqualTo(1));
		}

		[Test]
		public void ProcessBallots_MultipleBallots()
		{
			// Setup
			CreateSingleWeightedElectionConfig();
			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "1. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "   Dagobert Duck\r\n" +
			                                   "2. Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");
			ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "   Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "1. Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12346\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(2));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			TestUtils.CheckWeightedResult(election["Mickey Mouse"], 2);
			TestUtils.CheckWeightedResult(election["Donald Duck"], 0);
			TestUtils.CheckWeightedResult(election["Dagobert Duck"], 1);
			TestUtils.CheckWeightedResult(election["Daisy Duck"], 3);
		}

		[Test]
		public void ProcessBallots_Weighted_Valid_IncompleteTop()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "1. Mickey Mouse\r\n" +
			                                  "   Donald Duck\r\n" +
			                                  "   Dagobert Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			TestUtils.CheckWeightedResult(election["Mickey Mouse"], 2);
			TestUtils.CheckWeightedResult(election["Donald Duck"], 0);
			TestUtils.CheckWeightedResult(election["Dagobert Duck"], 0);
			TestUtils.CheckWeightedResult(election["Daisy Duck"], 0);
		}

		[Test]
		public void ProcessBallots_Weighted_Valid_IncompleteBottom()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "2. Mickey Mouse\r\n" +
			                                  "   Donald Duck\r\n" +
			                                  "   Dagobert Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			TestUtils.CheckWeightedResult(election["Mickey Mouse"], 1);
			TestUtils.CheckWeightedResult(election["Donald Duck"], 0);
			TestUtils.CheckWeightedResult(election["Dagobert Duck"], 0);
			TestUtils.CheckWeightedResult(election["Daisy Duck"], 0);
		}

		[Test]
		public void ProcessBallots_Weighted_Invalid_SameRankTwice()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "1. Mickey Mouse\r\n" +
			                                  "   Donald Duck\r\n" +
			                                  "1. Dagobert Duck\r\n" +
			                                  "   Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void ProcessBallots_Weighted_Invalid_SameNameTwice()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "1. Mickey Mouse\r\n" +
			                                  "   Donald Duck\r\n" +
			                                  "2. Mickey Mouse\r\n" +
			                                  "   Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void ProcessBallots_Weighted_Invalid_WrongName()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "1. Mickey Mouse\r\n" +
			                                  "   Donald Duck\r\n" +
			                                  "2. Dagobert Mouse\r\n" +
			                                  "   Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void ProcessBallots_YesNo_MultipleBallots()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [J] Mickey Mouse\r\n" +
			                                   "2. [N] Donald Duck\r\n" +
			                                   "3. [J] Dagobert Duck\r\n" +
			                                   "4. [E] Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");
			ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [N] Mickey Mouse\r\n" +
			                                   "2. [E] Donald Duck\r\n" +
			                                   "3. [J] Dagobert Duck\r\n" +
			                                   "4. [J] Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12346\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(2));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			TestUtils.CheckYesNoResult(election["Mickey Mouse"], 2, 1, 1, 0);
			TestUtils.CheckYesNoResult(election["Donald Duck"], 2, 0, 1, 1);
			TestUtils.CheckYesNoResult(election["Dagobert Duck"], 2, 2, 0, 0);
			TestUtils.CheckYesNoResult(election["Daisy Duck"], 2, 1, 0, 1);
		}

		[Test]
		public void ProcessBallots_YesNo_Invalid_SameNameTwice()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [J] Mickey Mouse\r\n" +
			                                  "2. [N] Donald Duck\r\n" +
			                                  "3. [N] Mickey Mouse\r\n" +
			                                  "4. [E] Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void ProcessBallots_YesNo_Invalid_IllegalMarking()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [X] Mickey Mouse\r\n" +
			                                  "2. [N] Donald Duck\r\n" +
			                                  "3. [J] Dagobert Duck\r\n" +
			                                  "4. [E] Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void ProcessBallots_YesNo_Valid_Incomplete_Space()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [ ] Mickey Mouse\r\n" +
			                                  "2. [N] Donald Duck\r\n" +
			                                  "3. [J] Dagobert Duck\r\n" +
			                                  "4. [E] Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			TestUtils.CheckYesNoResult(election["Mickey Mouse"], 1, 0, 0, 1);
			TestUtils.CheckYesNoResult(election["Donald Duck"], 1, 0, 1, 0);
			TestUtils.CheckYesNoResult(election["Dagobert Duck"], 1, 1, 0, 0);
			TestUtils.CheckYesNoResult(election["Daisy Duck"], 1, 0, 0, 1);
		}

		[Test]
		public void ProcessBallots_YesNo_Valid_Incomplete_DoubleSpace()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [  ] Mickey Mouse\r\n" +
			                                  "2. [N] Donald Duck\r\n" +
			                                  "3. [J] Dagobert Duck\r\n" +
			                                  "4. [E] Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			TestUtils.CheckYesNoResult(election["Mickey Mouse"], 1, 0, 0, 1);
			TestUtils.CheckYesNoResult(election["Donald Duck"], 1, 0, 1, 0);
			TestUtils.CheckYesNoResult(election["Dagobert Duck"], 1, 1, 0, 0);
			TestUtils.CheckYesNoResult(election["Daisy Duck"], 1, 0, 0, 1);
		}

		[Test]
		public void ProcessBallots_YesNo_Valid_Incomplete_NoSpace()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [] Mickey Mouse\r\n" +
			                                  "2. [N] Donald Duck\r\n" +
			                                  "3. [J] Dagobert Duck\r\n" +
			                                  "4. [E] Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			// Execute
			sut.ProcessBallots();

			// Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			Assert.That(sut.TalliedResults.Count, Is.EqualTo(1));

			var election = sut.TalliedResults.First().Value;
			TestUtils.CheckYesNoResult(election["Mickey Mouse"], 1, 0, 0, 1);
			TestUtils.CheckYesNoResult(election["Donald Duck"], 1, 0, 1, 0);
			TestUtils.CheckYesNoResult(election["Dagobert Duck"], 1, 1, 0, 0);
			TestUtils.CheckYesNoResult(election["Daisy Duck"], 1, 0, 0, 1);
		}

		[Test]
		public void GetResultString_Valid_OnlyOneNominee()
		{
			// Setup
			CreateSingleElectionWithOneCandidateConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
				"============\r\n" +
				"\r\n" +
				"Election\r\n" +
				"--------\r\n" +
				"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
				"2. Mickey Mouse\r\n" +
				"\r\n" +
				"\r\n" +
				"12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});
			sut.ProcessBallots();

			// Execute/Verify
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			Assert.That(sut.GetResultString(), Is.EqualTo($@"Election
--------
1. Mickey Mouse (1 points, 50%)
(1 ballots, thereof 0 invalid)
(max 2 points per candidate)
(max 3 points total, sum 1 points, 2 unallocated)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test]
		public void GetResultString_Weighted()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "2. Mickey Mouse\r\n" +
			                                  "   Donald Duck\r\n" +
			                                  "1. Dagobert Duck\r\n" +
			                                  "   Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});
			sut.ProcessBallots();
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			// Execute/Verify
			Assert.That(sut.GetResultString(), Is.EqualTo($@"Election
--------
1. Dagobert Duck (2 points, 100%)
2. Mickey Mouse (1 points, 50%)
   Daisy Duck (0 points, 0%)
   Donald Duck (0 points, 0%)
(1 ballots, thereof 0 invalid)
(max 2 points per candidate)
(max 3 points total, sum 3 points, 0 unallocated)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test]
		public void GetResultString_Weighted_SameVotes()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "2. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "1. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");
			ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "1. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});
			sut.ProcessBallots();
			Assert.That(sut.NumberOfBallots, Is.EqualTo(2));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			// Execute/Verify
			Assert.That(sut.GetResultString(), Is.EqualTo($@"Election
--------
1. Dagobert Duck (3 points, 75%)
1. Mickey Mouse (3 points, 75%)
   Daisy Duck (0 points, 0%)
   Donald Duck (0 points, 0%)
(2 ballots, thereof 0 invalid)
(max 4 points per candidate)
(max 6 points total, sum 6 points, 0 unallocated)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test]
		public void GetResultString_Weighted_Abstains()
		{
			// Setup
			CreateSingleWeightedElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
				"============\r\n" +
				"\r\n" +
				"Election\r\n" +
				"--------\r\n" +
				"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
				"   Mickey Mouse\r\n" +
				"   Donald Duck\r\n" +
				"   Dagobert Duck\r\n" +
				"   Daisy Duck\r\n" +
				"\r\n" +
				"\r\n" +
				"12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});
			sut.ProcessBallots();
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			// Execute/Verify
			Assert.That(sut.GetResultString(), Is.EqualTo($@"Election
--------
   Dagobert Duck (0 points, 0%)
   Daisy Duck (0 points, 0%)
   Donald Duck (0 points, 0%)
   Mickey Mouse (0 points, 0%)
(1 ballots, thereof 0 invalid)
(max 2 points per candidate)
(max 3 points total, sum 0 points, 3 unallocated)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test]
		public void GetResultString_YesNo()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [J] Mickey Mouse\r\n" +
			                                  "2. [N] Donald Duck\r\n" +
			                                  "3. [J] Dagobert Duck\r\n" +
			                                  "4. [E] Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			sut.ProcessBallots();
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			// Execute/Verify
			Assert.That(sut.GetResultString(), Is.EqualTo($@"Election
--------
Mickey Mouse: 1 J, 0 N, 0 E
Donald Duck: 0 J, 1 N, 0 E
Dagobert Duck: 1 J, 0 N, 0 E
Daisy Duck: 0 J, 0 N, 1 E
(1 ballots, thereof 0 invalid)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test]
		public void GetResultString_YesNo_DifferentOrder()
		{
			// Setup
			CreateSingleYesNoElectionConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [E] Daisy Duck\r\n" +
			                                  "2. [J] Dagobert Duck\r\n" +
			                                  "3. [N] Donald Duck\r\n" +
			                                  "4. [J] Mickey Mouse\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});

			sut.ProcessBallots();
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			// Execute/Verify
			Assert.That(sut.GetResultString(), Is.EqualTo($@"Election
--------
Mickey Mouse: 1 J, 0 N, 0 E
Donald Duck: 0 J, 1 N, 0 E
Dagobert Duck: 1 J, 0 N, 0 E
Daisy Duck: 0 J, 0 N, 1 E
(1 ballots, thereof 0 invalid)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test] public void TallyBallots()
		{
			// Setup
			CreateSingleElectionWithOneCandidateConfig();

			var ballotFileName = Path.Combine(_ballotsPath, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "2. Mickey Mouse\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			var sut = new BallotProcessor(new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory = _ballotsPath,
				PrintBallots = false,
				CreateCsv = false
			});
			sut.ProcessBallots();
			Assert.That(sut.NumberOfBallots, Is.EqualTo(1));
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			// Execute
			var result = sut.TallyBallots();

			// Verify
			Assert.That(result, Is.EqualTo($@"The election
============

Total of 1 ballots, thereof 0 at least partially invalid.

Election
--------
1. Mickey Mouse (1 points, 50%)
(1 ballots, thereof 0 invalid)
(max 2 points per candidate)
(max 3 points total, sum 1 points, 2 unallocated)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));

		}
	}
}