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
	public class WeightedElectionModelTests
	{
		[TestCase("Dagobert Duck\nMickey Mouse", false, ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
"2. Mickey Mouse\r\n" +
"   Donald Duck\r\n" +
"1. Dagobert Duck\r\n")]
		[TestCase("Donald Duck\nDagobert Duck", false, ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
"   Mickey Mouse\r\n" +
"1. Donald Duck\r\n" +
"2. Dagobert Duck\r\n")]
		[TestCase("", true, ExpectedResult = "Election\r\n" +
"--------\r\n" +
"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
"   Mickey Mouse\r\n" +
"   Donald Duck\r\n" +
"   Dagobert Duck\r\n")]
		public string GetResult(string electedNominees, bool writeEmptyBallot)
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var model = ElectionModelFactory.Create("Election", data);

			// Execute
			return model.GetResult(electedNominees.Split('\n').ToList(), writeEmptyBallot);
		}

		[Test]
		public void EmptyVotes()
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=Weighted
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var model = ElectionModelFactory.Create("Election", data);

			// Exercise/Verify
			Assert.That(model.EmptyVotes, Is.EqualTo(new[] { "Mickey Mouse", "Donald Duck"}));
		}
	}
}