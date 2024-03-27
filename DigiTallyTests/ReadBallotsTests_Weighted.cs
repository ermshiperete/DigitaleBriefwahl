// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class ReadBallotsTests_Weighted
	{
		private string _configFileName;
		private List<string> _ballotFileNames;

		internal static void CheckWeightedResult(CandidateResult result, int expectedPoints, [CallerLineNumber] int lineNumber = 0)
		{
			var weightedResult = result as WeightedCandidateResult;
			Assert.That(weightedResult?.Points, Is.EqualTo(expectedPoints),
				$"Expected {expectedPoints} points but got {weightedResult?.Points} in line {lineNumber}");
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
Typ=Weighted
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
		public void WeightedElection_SingleBallot()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckWeightedResult(election["Mickey Mouse"], 1);
			CheckWeightedResult(election["Donald Duck"], 0);
			CheckWeightedResult(election["Dagobert Duck"], 2);
			CheckWeightedResult(election["Daisy Duck"], 0);
		}

		[Test]
		public void WeightedElection_MultipleBallots()
		{
			// Setup
			var ballotFileName1 = Path.GetTempFileName();
			File.WriteAllText(ballotFileName1, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName1);
			var ballotFileName2 = Path.GetTempFileName();
			File.WriteAllText(ballotFileName2, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName2);

			// Execute
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName1), Is.True);
			Assert.That(sut.AddBallot(ballotFileName2), Is.True);

			// Verify
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckWeightedResult(election["Mickey Mouse"], 2);
			CheckWeightedResult(election["Donald Duck"], 0);
			CheckWeightedResult(election["Dagobert Duck"], 1);
			CheckWeightedResult(election["Daisy Duck"], 3);
		}

		[Test]
		public void WeightedElection_Valid_IncompleteTop()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckWeightedResult(election["Mickey Mouse"], 2);
			CheckWeightedResult(election["Donald Duck"], 0);
			CheckWeightedResult(election["Dagobert Duck"], 0);
			CheckWeightedResult(election["Daisy Duck"], 0);
		}

		[Test]
		public void WeightedElection_Valid_IncompleteBottom()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));
			var election = sut.Results.First().Value;

			CheckWeightedResult(election["Mickey Mouse"], 1);
			CheckWeightedResult(election["Donald Duck"], 0);
			CheckWeightedResult(election["Dagobert Duck"], 0);
			CheckWeightedResult(election["Daisy Duck"], 0);
		}

		[Test]
		public void WeightedElection_Invalid_SameRankTwice()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));
			var election = sut.Results.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void WeightedElection_Invalid_SameNameTwice()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));
			var election = sut.Results.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void WeightedElection_Invalid_WrongName()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));
			var election = sut.Results.First().Value;
			Assert.That(election.Keys.Count, Is.EqualTo(0));
		}

		[Test]
		public void GetResultString()
		{
			var ballotFileName = Path.GetTempFileName();
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
			_ballotFileNames.Add(ballotFileName);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.GetResultString(), Is.EqualTo(@"Election
--------
1. Dagobert Duck (2 points)
2. Mickey Mouse (1 points)
   Daisy Duck (0 points)
   Donald Duck (0 points)
(1 ballots, thereof 0 invalid)

"));
		}

		[Test]
		public void GetResultString_SameVotes()
		{
			var ballotFileName1 = Path.GetTempFileName();
			File.WriteAllText(ballotFileName1, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName1);
			var ballotFileName2 = Path.GetTempFileName();
			File.WriteAllText(ballotFileName2, "The election\r\n" +
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
			_ballotFileNames.Add(ballotFileName2);
			var sut = new ReadBallots(_configFileName);
			Assert.That(sut.AddBallot(ballotFileName1), Is.True);
			Assert.That(sut.AddBallot(ballotFileName2), Is.True);
			Assert.That(sut.GetResultString(), Is.EqualTo(@"Election
--------
1. Dagobert Duck (3 points)
1. Mickey Mouse (3 points)
   Daisy Duck (0 points)
   Donald Duck (0 points)
(2 ballots, thereof 0 invalid)

"));
		}
	}

}