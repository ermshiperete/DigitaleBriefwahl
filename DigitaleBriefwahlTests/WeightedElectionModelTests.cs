// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Linq;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahlTests
{
	[TestFixture]
	public class WeightedElectionModelTests
	{
		[TestCase("Dagobert Duck\nMickey Mouse", false, ExpectedResult = "Election\n" +
"--------\n" +
"2. Mickey Mouse\n" +
"   Donald Duck\n" +
"1. Dagobert Duck\n")]
		[TestCase("Donald Duck\nDagobert Duck", false, ExpectedResult = "Election\n" +
"--------\n" +
"   Mickey Mouse\n" +
"1. Donald Duck\n" +
"2. Dagobert Duck\n")]
		[TestCase("", true, ExpectedResult = "Election\n" +
"--------\n" +
"1. Mickey Mouse\n" +
"2. Donald Duck\n" +
"   Dagobert Duck\n")]
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
			var model = ElectionModelFactory.Create("Election", data) as WeightedElectionModel;

			// Execute
			return model.GetResult(electedNominees.Split('\n').ToList(), writeEmptyBallot);
		}
	}
}