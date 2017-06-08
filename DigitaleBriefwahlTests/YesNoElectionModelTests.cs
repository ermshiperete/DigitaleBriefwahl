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
		[TestCase("E\nN\nJ\nN", ExpectedResult = "Election\n" +
"--------\n" +
"1. [E] Mickey Mouse\n" +
"2. [N] Donald Duck\n" +
"3. [J] Dagobert Duck\n" +
"4. [N] Daisy Duck\n")]
		public string GetResult(string votes)
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
			var model = ElectionModelFactory.Create("Election", data) as YesNoElectionModel;

			// Execute
			return model.GetResult(votes.Split('\n').ToList());
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
			var model = ElectionModelFactory.Create("Election", data) as YesNoElectionModel;

			// Execute
			Assert.That(() => model.GetResult(votes.Split('\n').ToList()),
				Throws.Exception.TypeOf(expectedException));
		}
	}
}