// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class ReadBallotsTests
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
		public void AddBallot_SingleBallot()
		{
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
			var sut = new ReadBallots(_configFileName);

			Assert.That(sut.AddBallot(ballotFileName), Is.True);

			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(0));

			var election1 = sut.Results.First();
			Assert.That(election1.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election1.Key.Invalid, Is.EqualTo(0));
			var election1Result = election1.Value;
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Mickey Mouse"], 1);
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Donald Duck"], 0);
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Dagobert Duck"], 2);
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Daisy Duck"], 0);

			var election2 = sut.Results.Last();
			Assert.That(election2.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election2.Key.Invalid, Is.EqualTo(0));
			var election2Result = election2.Value;
			ReadBallotsTests_YesNo.CheckYesNoResult(election2Result["One"], 1, 1, 0, 0);
			ReadBallotsTests_YesNo.CheckYesNoResult(election2Result["Two"], 1, 0, 1, 0);
			ReadBallotsTests_YesNo.CheckYesNoResult(election2Result["Three"], 1, 1, 0, 0);
			ReadBallotsTests_YesNo.CheckYesNoResult(election2Result["Four"], 1, 0, 0, 1);
		}

		[Test]
		public void AddBallot_OneInvalid()
		{
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
			var sut = new ReadBallots(_configFileName);

			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			var election1 = sut.Results.First();
			Assert.That(election1.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election1.Key.Invalid, Is.EqualTo(0));
			var election1Result = election1.Value;
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Mickey Mouse"], 1);
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Donald Duck"], 0);
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Dagobert Duck"], 2);
			ReadBallotsTests_Weighted.CheckWeightedResult(election1Result["Daisy Duck"], 0);

			var election2 = sut.Results.Last();
			Assert.That(election2.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election2.Key.Invalid, Is.EqualTo(1));
		}

		[Test]
		public void AddBallot_BothInvalid()
		{
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
			var sut = new ReadBallots(_configFileName);

			Assert.That(sut.AddBallot(ballotFileName), Is.True);
			Assert.That(sut.NumberOfInvalidBallots, Is.EqualTo(1));

			var election1 = sut.Results.First();
			Assert.That(election1.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election1.Key.Invalid, Is.EqualTo(1));

			var election2 = sut.Results.Last();
			Assert.That(election2.Key.BallotsProcessed, Is.EqualTo(1));
			Assert.That(election2.Key.Invalid, Is.EqualTo(1));
		}
	}
}