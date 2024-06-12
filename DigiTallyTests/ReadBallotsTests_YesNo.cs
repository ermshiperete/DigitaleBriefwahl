// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DigitaleBriefwahl.Model;
using NUnit.Framework;
using SIL.Providers;
using SIL.TestUtilities.Providers;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class ReadBallotsTests_YesNo
	{
		private string _configFileName;
		private List<string> _ballotFileNames;

		internal static void CheckYesNoResult(CandidateResult result, int expectedTotal, int expectedYes, int expectedNo, int expectedAbstain, [CallerLineNumber] int lineNumber = 0)
		{
			var ynResult = result as YesNoCandidateResult;
			Assert.That(ynResult.Yes, Is.EqualTo(expectedYes),
				$"Expected {expectedYes} Yes but got {ynResult.Yes} in line {lineNumber}");
			Assert.That(ynResult.No, Is.EqualTo(expectedNo),
				$"Expected {expectedNo} No but got {ynResult.No} in line {lineNumber}");
			Assert.That(ynResult.Abstention, Is.EqualTo(expectedAbstain),
				$"Expected {expectedAbstain} Abstain but got {ynResult.Abstention} in line {lineNumber}");
			Assert.That(ynResult.Yes + ynResult.No + ynResult.Abstention, Is.EqualTo(expectedTotal),
				$"Expected {expectedTotal} votes, but got {ynResult.Yes + ynResult.No + ynResult.Abstention} in line {lineNumber}");
		}

		[SetUp]
		public void Setup()
		{
			_ballotFileNames = new List<string>();
			_configFileName = Path.GetTempFileName();
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

		[TearDown]
		public void Teardown()
		{
			File.Delete(_configFileName);
			foreach (var file in _ballotFileNames)
				File.Delete(file);
		}

		[Test]
		public void YesNoElection_SingleBallot()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckYesNoResult(election["Mickey Mouse"], 1, 1, 0, 0);
			CheckYesNoResult(election["Donald Duck"], 1, 0, 1, 0);
			CheckYesNoResult(election["Dagobert Duck"], 1, 1, 0, 0);
			CheckYesNoResult(election["Daisy Duck"], 1, 0, 0, 1);
		}

		[Test]
		public void YesNoElection_MultipleBallots()
		{
			// Setup
			var ballotFileName1 = Path.GetTempFileName();
			File.WriteAllText(ballotFileName1, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName1);
			var ballotFileName2 = Path.GetTempFileName();
			File.WriteAllText(ballotFileName2, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName2);

			// Execute
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName1), Is.True);
			Assert.That(sut.AddBallot(ballotFileName2), Is.True);

			// Verify
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckYesNoResult(election["Mickey Mouse"], 2, 1, 1, 0);
			CheckYesNoResult(election["Donald Duck"], 2, 0, 1, 1);
			CheckYesNoResult(election["Dagobert Duck"], 2, 2, 0, 0);
			CheckYesNoResult(election["Daisy Duck"], 2, 1, 0, 1);
		}

		[Test]
		public void YesNoElection_Invalid_SameNameTwice()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));
			var election = sut.Results.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void YesNoElection_Invalid_IllegalMarking()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));
			var election = sut.Results.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void YesNoElection_Valid_Incomplete_Space()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckYesNoResult(election["Mickey Mouse"], 1, 0, 0, 1);
			CheckYesNoResult(election["Donald Duck"], 1, 0, 1, 0);
			CheckYesNoResult(election["Dagobert Duck"], 1, 1, 0, 0);
			CheckYesNoResult(election["Daisy Duck"], 1, 0, 0, 1);
		}

		[Test]
		public void YesNoElection_Valid_Incomplete_DoubleSpace()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckYesNoResult(election["Mickey Mouse"], 1, 0, 0, 1);
			CheckYesNoResult(election["Donald Duck"], 1, 0, 1, 0);
			CheckYesNoResult(election["Dagobert Duck"], 1, 1, 0, 0);
			CheckYesNoResult(election["Daisy Duck"], 1, 0, 0, 1);
		}

		[Test]
		public void YesNoElection_Valid_Incomplete_NoSpace()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckYesNoResult(election["Mickey Mouse"], 1, 0, 0, 1);
			CheckYesNoResult(election["Donald Duck"], 1, 0, 1, 0);
			CheckYesNoResult(election["Dagobert Duck"], 1, 1, 0, 0);
			CheckYesNoResult(election["Daisy Duck"], 1, 0, 0, 1);
		}

		[Test]
		public void GetResultString()
		{
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
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
		public void GetResultString_DifferentOrder()
		{
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2024, 6,
				12, 15, 32, 0)));
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
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

	}
}