// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using NUnit.Framework;
using SIL.Providers;
using SIL.TestUtilities.Providers;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class AcceptanceTests
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
		public void EndToEndConsoleOutput_WithBallotPrinting()
		{
			// Setup
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Election1
Wahl2=Election2
Email=election@example.com
PublicKey=12345678.asc

[Election1]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck

[Election2]
Text=Some description
Typ=YesNo
Stimmen=2
Kandidat1=One
Kandidat2=Two
Kandidat3=Three
Kandidat4=Four
");
			// Write ballots
			var ballotFileName1 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName1, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "2. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "1. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [J] One\r\n" +
			                                   "2. [N] Two\r\n" +
			                                   "3. [J] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName2 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName2, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "   Mickey Mouse\r\n" +
			                                   "1. Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [N] One\r\n" +
			                                   "2. [J] Two\r\n" +
			                                   "3. [E] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName3 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName3, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "1. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [X] One\r\n" +
			                                   "2. [E] Two\r\n" +
			                                   "3. [N] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = true,
				CreateCsv = false
			};

			// Execute
			var ballotProcessor = new BallotProcessor(options);
			var output = ballotProcessor.GetElectionResults();

			// Verify
			Assert.That(output, Is.EqualTo(@$"



          D1

          The election
          ============

          Election1
          --------
          (2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)
          2. Mickey Mouse
             Donald Duck
          1. Dagobert Duck
             Daisy Duck

          Election2
          --------
          (J=Ja, E=Enthaltung, N=Nein)
          1. [J] One
          2. [N] Two
          3. [J] Three
          4. [E] Four


          12345
{'\f'}




          D2

          The election
          ============

          Election1
          --------
          (2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)
             Mickey Mouse
          1. Donald Duck
          2. Dagobert Duck
             Daisy Duck

          Election2
          --------
          (J=Ja, E=Enthaltung, N=Nein)
          1. [N] One
          2. [J] Two
          3. [E] Three
          4. [E] Four


          12345
{'\f'}




          D3

          The election
          ============

          Election1
          --------
          (2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)
          1. Mickey Mouse
             Donald Duck
          2. Dagobert Duck
             Daisy Duck

          Election2
          --------
          (J=Ja, E=Enthaltung, N=Nein)
          1. [X] One
          2. [E] Two
          3. [N] Three
          4. [E] Four


          12345
{'\f'}




          The election
          ============

          Total of 3 ballots, thereof 1 at least partially invalid.

          Election1
          ---------
          1. Dagobert Duck (4 points, 66%)
          2. Mickey Mouse (3 points, 50%)
             Donald Duck (2 points, 33%)
             Daisy Duck (0 points, 0%)
          (3 ballots, thereof 0 invalid)
          (max 6 points per candidate)
          (max 9 points total, sum 9 points, 0 unallocated)

          Election2
          ---------
          One: 1 J, 1 N, 0 E
          Two: 1 J, 1 N, 0 E
          Three: 1 J, 0 N, 1 E
          Four: 0 J, 0 N, 2 E
          (3 ballots, thereof 1 invalid)

          (DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test]
		public void EndToEndConsoleOutput_WithoutBallotPrinting()
		{
			// Setup
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Election1
Wahl2=Election2
Email=election@example.com
PublicKey=12345678.asc

[Election1]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck

[Election2]
Text=Some description
Typ=YesNo
Stimmen=2
Kandidat1=One
Kandidat2=Two
Kandidat3=Three
Kandidat4=Four
");
			// Write ballots
			var ballotFileName1 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName1, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "2. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "1. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [J] One\r\n" +
			                                   "2. [N] Two\r\n" +
			                                   "3. [J] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName2 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName2, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "   Mickey Mouse\r\n" +
			                                   "1. Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [N] One\r\n" +
			                                   "2. [J] Two\r\n" +
			                                   "3. [E] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName3 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName3, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "1. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [X] One\r\n" +
			                                   "2. [E] Two\r\n" +
			                                   "3. [N] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = false
			};

			// Execute
			var ballotProcessor = new BallotProcessor(options);
			var output = ballotProcessor.GetElectionResults();

			// Verify
			Assert.That(output, Is.EqualTo(@$"The election
============

Total of 3 ballots, thereof 1 at least partially invalid.

Election1
---------
1. Dagobert Duck (4 points, 66%)
2. Mickey Mouse (3 points, 50%)
   Donald Duck (2 points, 33%)
   Daisy Duck (0 points, 0%)
(3 ballots, thereof 0 invalid)
(max 6 points per candidate)
(max 9 points total, sum 9 points, 0 unallocated)

Election2
---------
One: 1 J, 1 N, 0 E
Two: 1 J, 1 N, 0 E
Three: 1 J, 0 N, 1 E
Four: 0 J, 0 N, 2 E
(3 ballots, thereof 1 invalid)

(DigiTally version {GitVersionInformation.SemVer}; report executed 2024-06-12 15:32)
"));
		}

		[Test]
		public void EndToEndCsvFile_FullExample()
		{
			// Setup
			// Write config
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Ehrenamtlicher Vorstand
Wahl2=Fachvorstand
Wahl3=Vorstandsvorsitzender
Email=election@example.com
PublicKey=12345678.asc

[Ehrenamtlicher Vorstand]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck

[Fachvorstand]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Bugs Bunny
Kandidat2=Duffy Duck
Kandidat3=Porky Pig
Kandidat4=Elmer Fudd

[Vorstandsvorsitzender]
Text=Some description
Typ=YesNo
Stimmen=1
Kandidat1=Tarzan
");
			// Write ballots
			var ballotFileName1 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName1, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Ehrenamtlicher Vorstand\r\n" +
			                                   "------------------------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "2. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "1. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Fachvorstand\r\n" +
			                                   "--------------\r\n" +
			                                   "(3 Stimmen; Wahl der Reihenfolge nach mit 1.-3." +
											   " kennzeichnen)\r\n" +
											   "1. Bugs Bunny\r\n" +
											   "2. Duffy Duck\r\n" +
											   "3. Porky Pig\r\n" +
											   "   Elmer Fudd\r\n" +
			                                   "\r\n" +
											   "Vorstandsvorsitzender\r\n" +
			                                   "----------------------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [J] Tarzan\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName2 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName2, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Ehrenamtlicher Vorstand\r\n" +
			                                   "------------------------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "   Mickey Mouse\r\n" +
			                                   "1. Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Fachvorstand\r\n" +
			                                   "--------------\r\n" +
			                                   "(3 Stimmen; Wahl der Reihenfolge nach mit 1.-3." +
											   " kennzeichnen)\r\n" +
											   "1. Bugs Bunny\r\n" +
											   "   Duffy Duck\r\n" +
											   "3. Porky Pig\r\n" +
											   "2. Elmer Fudd\r\n" +
			                                   "\r\n" +
											   "Vorstandsvorsitzender\r\n" +
			                                   "----------------------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [E] Tarzan\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName3 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName3, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Ehrenamtlicher Vorstand\r\n" +
			                                   "------------------------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "1. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "   Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Fachvorstand\r\n" +
			                                   "--------------\r\n" +
			                                   "(3 Stimmen; Wahl der Reihenfolge nach mit 1.-3." +
											   " kennzeichnen)\r\n" +
											   "   Bugs Bunny\r\n" +
											   "2. Duffy Duck\r\n" +
											   "1. Porky Pig\r\n" +
											   "3. Elmer Fudd\r\n" +
			                                   "\r\n" +
											   "Vorstandsvorsitzender\r\n" +
			                                   "----------------------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [N] Tarzan\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = true
			};
			var ballotProcessor = new BallotProcessor(options);

			// Execute
			ballotProcessor.ProcessBallots();

			// Verify
			var csvFile = ballotProcessor.CsvFile;
			Assert.That(csvFile, Is.EqualTo("The_election.csv"));
			Assert.That(File.Exists(csvFile), Is.True);
			var content = File.ReadAllText(csvFile);
			Assert.That(content, Is.EqualTo(@"Nummer;;Mickey Mouse;Donald Duck;Dagobert Duck;Daisy Duck;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Ehrenamtl. Vorstand inkomplett;;Bugs Bunny;Duffy Duck;Porky Pig;Elmer Fudd;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Name Kandidat 8;Name Kandidat 9;Name Kandidat 10;Name Kandidat 11;Name Kandidat 12;Name Kandidat 13;Name Kandidat 14;Name Kandidat 15;Fachvorstand inkomplett;;Ja;Nein;Enth.;
1;;2;;1;;;;;;;1;2;3;;;;;;;;;;;;;;;1;;;
2;;;1;2;;;;;;;1;;3;2;;;;;;;;;;;;;;;;1;
3;;1;;;;;;;1;;;2;1;3;;;;;;;;;;;;;;;1;;
"));
		}

		[Test]
		public void EndToEndCsvFile_PartialExample()
		{
			// Setup
			// Write config
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Fachvorstand
Email=election@example.com
PublicKey=12345678.asc

[Fachvorstand]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Bugs Bunny
Kandidat2=Duffy Duck
Kandidat3=Porky Pig
Kandidat4=Elmer Fudd
");
			// Write ballots
			var ballotFileName1 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName1, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Fachvorstand\r\n" +
			                                   "--------------\r\n" +
			                                   "(3 Stimmen; Wahl der Reihenfolge nach mit 1.-3." +
											   " kennzeichnen)\r\n" +
											   "1. Bugs Bunny\r\n" +
											   "2. Duffy Duck\r\n" +
											   "3. Porky Pig\r\n" +
											   "   Elmer Fudd\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName2 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName2, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Fachvorstand\r\n" +
			                                   "--------------\r\n" +
			                                   "(3 Stimmen; Wahl der Reihenfolge nach mit 1.-3." +
											   " kennzeichnen)\r\n" +
											   "1. Bugs Bunny\r\n" +
											   "   Duffy Duck\r\n" +
											   "3. Porky Pig\r\n" +
											   "2. Elmer Fudd\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName3 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName3, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Fachvorstand\r\n" +
			                                   "--------------\r\n" +
			                                   "(3 Stimmen; Wahl der Reihenfolge nach mit 1.-3." +
											   " kennzeichnen)\r\n" +
											   "   Bugs Bunny\r\n" +
											   "2. Duffy Duck\r\n" +
											   "1. Porky Pig\r\n" +
											   "3. Elmer Fudd\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = true
			};
			var ballotProcessor = new BallotProcessor(options);

			// Execute
			ballotProcessor.ProcessBallots();

			// Verify
			var csvFile = ballotProcessor.CsvFile;
			Assert.That(File.Exists(csvFile), Is.True);
			var content = File.ReadAllText(csvFile);
			Assert.That(content, Is.EqualTo(@"Nummer;;Name Kandidat 1;Name Kandidat 2;Name Kandidat 3;Name Kandidat 4;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Ehrenamtl. Vorstand inkomplett;;Bugs Bunny;Duffy Duck;Porky Pig;Elmer Fudd;Name Kandidat 5;Name Kandidat 6;Name Kandidat 7;Name Kandidat 8;Name Kandidat 9;Name Kandidat 10;Name Kandidat 11;Name Kandidat 12;Name Kandidat 13;Name Kandidat 14;Name Kandidat 15;Fachvorstand inkomplett;;Ja;Nein;Enth.;
1;;;;;;;;;;;1;2;3;;;;;;;;;;;;;;;;;;
2;;;;;;;;;;;1;;3;2;;;;;;;;;;;;;;;;;
3;;;;;;;;;;;;2;1;3;;;;;;;;;;;;;;;;;
"));
		}

		[Test]
		public void EndToEndCsvFile_InvalidElection()
		{
			// Setup
			// Write config
			File.WriteAllText(_configFileName, @"[Wahlen]
Titel=The election
Wahl1=Election1
Email=election@example.com
PublicKey=12345678.asc

[Election1]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck
");
			// Write ballots
			var ballotFileName1 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName1, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "---------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "2. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "1. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
			var options = new DigiTallyOptions {
				ConfigFile = _configFileName,
				BallotsDirectory =  _ballotDirectoryName,
				PrintBallots = false,
				CreateCsv = true
			};
			var ballotProcessor = new BallotProcessor(options);

			// Execute
			ballotProcessor.ProcessBallots();

			// Verify
			var csvFile = ballotProcessor.CsvFile;
			Assert.That(csvFile, Is.Null);
		}
	}
}