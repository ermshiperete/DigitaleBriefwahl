using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class ReadBallotTests_YesNo
	{
		private string       _configFileName;
		private List<string> _ballotFileNames;

		private static void CheckYesNoResult(ElectionResult result, int expectedTotal,
			int expectedYes, int expectedNo, int expectedAbstain,
			int expectedInvalid = 0, [CallerLineNumber] int lineNumber = 0)
		{
			var ynResult = result as YesNoElectionResult;
			Assert.That(ynResult.Yes, Is.EqualTo(expectedYes),
				$"Expected {expectedYes} Yes but got {ynResult.Yes} in line {lineNumber}");
			Assert.That(ynResult.No, Is.EqualTo(expectedNo),
				$"Expected {expectedNo} No but got {ynResult.No} in line {lineNumber}");
			Assert.That(ynResult.Abstain, Is.EqualTo(expectedAbstain),
				$"Expected {expectedAbstain} Abstain but got {ynResult.Abstain} in line {lineNumber}");
			Assert.That(result.Invalid, Is.EqualTo(expectedInvalid),
				$"Expected {expectedInvalid} Invalid but got {result.Invalid} in line {lineNumber}");
			Assert.That(ynResult.Yes + ynResult.No + ynResult.Abstain + ynResult.Invalid, Is.EqualTo(expectedTotal),
				$"Expected {expectedTotal} votes, but got {ynResult.Yes + ynResult.No + ynResult.Abstain + ynResult.Invalid} in line {lineNumber}");
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

			CheckYesNoResult(election["Mickey Mouse"], 2, 1, 1, 0, 0);
			CheckYesNoResult(election["Donald Duck"], 2, 0, 1, 1, 0);
			CheckYesNoResult(election["Dagobert Duck"], 2, 2, 0, 0, 0);
			CheckYesNoResult(election["Daisy Duck"], 2, 1, 0, 1, 0);
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