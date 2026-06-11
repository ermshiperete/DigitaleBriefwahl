// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class ReadBallotTests
	{
		private string _configFileName;
		private List<string> _ballotFileNames;

		[SetUp]
		public void Setup()
		{
			_ballotFileNames = new List<string>();
			_configFileName = Path.GetTempFileName();
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
		}

		[TearDown]
		public void Teardown()
		{
			File.Delete(_configFileName);
			foreach (var file in _ballotFileNames)
				File.Delete(file);
		}

		[Test]
		public void PrintBallot()
		{
			// Setup
			var ballotFileName = Path.GetTempFileName();
			File.WriteAllText(ballotFileName, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName);
			var configuration = Configuration.Configure(_configFileName);
			var sut = new ReadBallot(configuration, ballotFileName);

			// Execute
			var result = sut.PrintBallot(1);

			// Verify
			Assert.That(result, Is.EqualTo(@"



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
" + "\f"));
		}

		[Test]
		public void AddBallot_SingleBallot()
		{
			// Setup
			var ballotFileName = Path.GetTempFileName();
			File.WriteAllText(ballotFileName, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName);
			var configuration = Configuration.Configure(_configFileName);
			var talliedResults = new Dictionary<ElectionModel, Dictionary<string,
				CandidateResult>>();
			var sut = new ReadBallot(configuration, ballotFileName);

			// Execute
			var (invalid, ballotResults) = sut.AddBallot(talliedResults);

			// Verify
			Assert.That(invalid, Is.False);
			Assert.That(talliedResults.Count, Is.EqualTo(2));

			var election1 = talliedResults.First();
			Assert.That(election1.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election1.Key.Invalid, Is.EqualTo(0));
			var election1Result = election1.Value;
			TestUtils.CheckWeightedResult(election1Result["Mickey Mouse"], 1);
			TestUtils.CheckWeightedResult(election1Result["Donald Duck"], 0);
			TestUtils.CheckWeightedResult(election1Result["Dagobert Duck"], 2);
			TestUtils.CheckWeightedResult(election1Result["Daisy Duck"], 0);

			var election2 = talliedResults.Last();
			Assert.That(election2.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election2.Key.Invalid, Is.EqualTo(0));
			var election2Result = election2.Value;
			TestUtils.CheckYesNoResult(election2Result["One"], 1, 1, 0, 0);
			TestUtils.CheckYesNoResult(election2Result["Two"], 1, 0, 1, 0);
			TestUtils.CheckYesNoResult(election2Result["Three"], 1, 1, 0, 0);
			TestUtils.CheckYesNoResult(election2Result["Four"], 1, 0, 0, 1);
		}

		[Test]
		public void AddBallot_OneInvalid()
		{
			// Setup
			var ballotFileName = Path.GetTempFileName();
			File.WriteAllText(ballotFileName, "The election\r\n" +
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
			                                  "2. [X] Two\r\n" +
			                                  "3. [J] Three\r\n" +
			                                  "4. [E] Four\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			_ballotFileNames.Add(ballotFileName);
			var configuration = Configuration.Configure(_configFileName);
			var talliedResults = new Dictionary<ElectionModel, Dictionary<string,
				CandidateResult>>();
			var sut = new ReadBallot(configuration, ballotFileName);

			// Execute
			var (invalid, ballotResults) = sut.AddBallot(talliedResults);

			// Verify
			Assert.That(invalid, Is.True);
			Assert.That(talliedResults.Count, Is.EqualTo(2));

			var election1 = talliedResults.First();
			Assert.That(election1.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election1.Key.Invalid, Is.EqualTo(0));
			var election1Result = election1.Value;
			TestUtils.CheckWeightedResult(election1Result["Mickey Mouse"], 1);
			TestUtils.CheckWeightedResult(election1Result["Donald Duck"], 0);
			TestUtils.CheckWeightedResult(election1Result["Dagobert Duck"], 2);
			TestUtils.CheckWeightedResult(election1Result["Daisy Duck"], 0);

			var election2 = talliedResults.Last();
			Assert.That(election2.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election2.Key.Invalid, Is.EqualTo(1));
		}

		[Test]
		public void AddBallot_BothInvalid()
		{
			// Setup
			var ballotFileName = Path.GetTempFileName();
			File.WriteAllText(ballotFileName, "The election\r\n" +
			                                  "============\r\n" +
			                                  "\r\n" +
			                                  "Election1\r\n" +
			                                  "--------\r\n" +
			                                  "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                  "1. Mickey Mouse\r\n" +
			                                  "   Donald Duck\r\n" +
			                                  "1. Dagobert Duck\r\n" +
			                                  "   Daisy Duck\r\n" +
			                                  "\r\n" +
			                                  "Election2\r\n" +
			                                  "--------\r\n" +
			                                  "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                  "1. [J] One\r\n" +
			                                  "2. [X] Two\r\n" +
			                                  "3. [J] Three\r\n" +
			                                  "4. [E] Four\r\n" +
			                                  "\r\n" +
			                                  "\r\n" +
			                                  "12345\r\n");
			_ballotFileNames.Add(ballotFileName);
			var configuration = Configuration.Configure(_configFileName);
			var talliedResults = new Dictionary<ElectionModel, Dictionary<string,
				CandidateResult>>();
			var sut = new ReadBallot(configuration, ballotFileName);

			// Execute
			var (invalid, ballotResults) = sut.AddBallot(talliedResults);

			// Verify
			Assert.That(invalid, Is.True);
			Assert.That(talliedResults.Count, Is.EqualTo(2));

			var election1 = talliedResults.First();
			Assert.That(election1.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election1.Key.Invalid, Is.EqualTo(1));

			var election2 = talliedResults.Last();
			Assert.That(election2.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election2.Key.Invalid, Is.EqualTo(1));
		}
	}
}