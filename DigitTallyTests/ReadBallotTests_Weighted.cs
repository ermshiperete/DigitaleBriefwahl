using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DigitaleBriefwahl.Model;
using DigitaleBriefwahl.Tally;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class ReadBallotTests_Weighted
	{
		private string       _configFileName;
		private List<string> _ballotFileNames;

		private static void CheckWeightedResult(ElectionResult result, int expectedPoints,
			int expectedInvalid = 0, [CallerLineNumber] int lineNumber = 0)
		{
			var weightedResult = result as WeightedElectionResult;
			Assert.That(weightedResult.Points, Is.EqualTo(expectedPoints),
				$"Expected {expectedPoints} points but got {weightedResult.Points} in line {lineNumber}");
			Assert.That(result.Invalid, Is.EqualTo(expectedInvalid),
				$"Expected {expectedInvalid} Invalid but got {result.Invalid} in line {lineNumber}");
		}

		[SetUp]
		public void Setup()
		{
			_ballotFileNames = new List<string>();
			_configFileName = Path.GetTempFileName();
			File.WriteAllText(_configFileName, @"[Wahlen]
Title=The election
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
				$"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
				"2. Mickey Mouse\r\n" +
				"   Donald Duck\r\n" +
				"1. Dagobert Duck\r\n" +
				"   Daisy Duck\r\n" +
				"\r\n" +
				"\r\n" +
				"12345\r\n");
			_ballotFileNames.Add(ballotFileName);
			var ballot = new ReadBallot(_configFileName);
			ballot.AddBallot(ballotFileName);
			Assert.That(ballot.IsReadable, Is.True);
			var result = ballot.Results;
			Assert.That(result.Keys.First().Invalid, Is.EqualTo(0));
			var election = result.First().Value;

			CheckWeightedResult(election["Mickey Mouse"], 1, 0);
			CheckWeightedResult(election["Donald Duck"], 0, 0);
			CheckWeightedResult(election["Dagobert Duck"], 2, 0);
			CheckWeightedResult(election["Daisy Duck"], 0, 0);
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
											$"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
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
											$"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
											"   Mickey Mouse\r\n" +
											"   Donald Duck\r\n" +
											"2. Dagobert Duck\r\n" +
											"1. Daisy Duck\r\n" +
											"\r\n" +
											"\r\n" +
											"12346\r\n");
			_ballotFileNames.Add(ballotFileName2);

			// Execute
			var sut = new ReadBallot(_configFileName);
			sut.AddBallot(ballotFileName1);
			sut.AddBallot(ballotFileName2);

			// Verify
			Assert.That(sut.IsReadable, Is.True);
			var result = sut.Results;
			Assert.That(result.Keys.First().Invalid, Is.EqualTo(0));
			var election = result.First().Value;

			CheckWeightedResult(election["Mickey Mouse"], 2, 0);
			CheckWeightedResult(election["Donald Duck"], 0, 0);
			CheckWeightedResult(election["Dagobert Duck"], 1, 0);
			CheckWeightedResult(election["Daisy Duck"], 3, 0);
		}

		[Test]
		public void WeightedElection_Invalid()
		{
			Assert.Fail();
		}
	}

	/*
	 - Wahl 1:
		Kandidat 1:
		    5 J, 3 N, 1 E
		Kandidate 2:
			J, N, E
	 - Wahl 2:
	    Kandidate 1:
	       88 Punkte
	    Kandidat 2:
	       85 Punkte
	 */
}