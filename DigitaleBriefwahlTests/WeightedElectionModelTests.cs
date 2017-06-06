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
		[TestCase("Dagobert Duck\nMickey Mouse", ExpectedResult = @"Election
--------
2. Mickey Mouse
   Donald Duck
1. Dagobert Duck
")]
		[TestCase("Donald Duck\nDagobert Duck", ExpectedResult = @"Election
--------
   Mickey Mouse
1. Donald Duck
2. Dagobert Duck
")]
		public string GetResult(string electedNominees)
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
			return model.GetResult(electedNominees.Split('\n').ToList());
		}
	}
}