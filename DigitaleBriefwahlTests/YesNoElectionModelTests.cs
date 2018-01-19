// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Linq;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahlTests
{
	[TestFixture]
	public class YesNoElectionModelTests
	{
		[TestCase("E\nN\nJ\nN", false, ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(J=Ja, E=Enthaltung, N=Nein)\r\n" +
"1. [E] Mickey Mouse\r\n" +
"2. [N] Donald Duck\r\n" +
"3. [J] Dagobert Duck\r\n" +
"4. [N] Daisy Duck\r\n")]
		[TestCase("E\nE\nE\nE", true, ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(J=Ja, E=Enthaltung, N=Nein)\r\n" +
"1. [E] Mickey Mouse\r\n" +
"2. [E] Donald Duck\r\n" +
"3. [E] Dagobert Duck\r\n" +
"4. [E] Daisy Duck\r\n")]
		public string GetResult_TwoVotes(string votes, bool writeEmptyBallot)
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=YesNo
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var model = ElectionModelFactory.Create("Election", data);

			// Execute
			return model.GetResult(votes.Split('\n').ToList(), writeEmptyBallot);
		}

		[TestCase("J", ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(J=Ja, E=Enthaltung, N=Nein)\r\n" +
"1. [J] Mickey Mouse\r\n")]
		[TestCase("E", ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(J=Ja, E=Enthaltung, N=Nein)\r\n" +
"1. [E] Mickey Mouse\r\n")]
		[TestCase("N", ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(J=Ja, E=Enthaltung, N=Nein)\r\n" +
"1. [N] Mickey Mouse\r\n")]
		public string GetResult_OneVote(string votes)
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=YesNo
Stimmen=1
Kandidat1=Mickey Mouse
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var model = ElectionModelFactory.Create("Election", data);

			// Execute
			return model.GetResult(votes.Split('\n').ToList(), false);
		}

		[TestCase("", typeof(ArgumentException))]
		[TestCase("E\nJ", typeof(ArgumentException), Description = "Invalid number of votes")]
		[TestCase("Ne\nJa\nEn\nJa", typeof(ArgumentException), Description = "Invalid input string (exactly one character expected per vote)")]
		[TestCase("J\nJ\nJ\nJ", typeof(ArgumentException), Description = "Number of yes votes doesn't match allowed number of votes")]
		[TestCase("E\nE\nN\nJ", typeof(ArgumentException), Description = "Abstain counts as vote")]
		[TestCase("Y\nE\nN\nJ", typeof(ArgumentException), Description = "Invalid vote (allowed are J, N, E)")]
		public void GetResult_InvalidInput(string votes, Type expectedException)
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=YesNo
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
Kandidat4=Daisy Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var model = ElectionModelFactory.Create("Election", data);

			// Execute
			Assert.That(() => model.GetResult(votes.Split('\n').ToList(), false),
				Throws.Exception.TypeOf(expectedException));
		}
	}
}