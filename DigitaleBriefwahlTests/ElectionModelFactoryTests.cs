// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using DigitaleBriefwahl;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahlTests
{
	[TestFixture]
	public class ElectionModelFactoryTests
	{
		[TestCase("YesNo", ExpectedResult = typeof(YesNoElectionModel))]
		[TestCase("Weighted", ExpectedResult = typeof(WeightedElectionModel))]
		public System.Type Create(string type)
		{
			// Setup
			var ini = $@"[Election]
Text=Some description
Typ={type}
Stimmen=1
Kandidat1=Mickey Mouse
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);

			// Execute
			return ElectionModelFactory.Create("Election", data).GetType();
		}

		[TestCase("Unknown")]
		[TestCase(null)]
		[TestCase("")]
		public void Create_Invalid(string type)
		{
			// Setup
			var ini = $@"[Election]
Text=Some description
Typ={type}
Stimmen=1
Kandidat1=Mickey Mouse
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);

			// Execute
			Assert.That(() => ElectionModelFactory.Create("Election", data),
				Throws.TypeOf<InvalidConfigurationException>());
		}
	}
}