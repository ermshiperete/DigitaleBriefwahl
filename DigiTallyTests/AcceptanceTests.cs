// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace DigitaleBriefwahl.Tally.Tests
{
	[TestFixture]
	public class AcceptanceTests
	{
		private string _configFileName;
		private string _ballotDirectoryName;

		[SetUp]
		public void Setup()
		{
			_ballotDirectoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_ballotDirectoryName);
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
			Directory.Delete(_ballotDirectoryName, true);
		}

		[Test]
		public void EndToEnd()
		{
			var ballotFileName1 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName1, "The election\r\n" +
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

			var ballotFileName2 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName2, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "   Mickey Mouse\r\n" +
			                                   "1. Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [N] One\r\n" +
			                                   "2. [J] Two\r\n" +
			                                   "3. [E] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var ballotFileName3 = Path.Combine(_ballotDirectoryName, Path.GetRandomFileName());
			File.WriteAllText(ballotFileName3, "The election\r\n" +
			                                   "============\r\n" +
			                                   "\r\n" +
			                                   "Election1\r\n" +
			                                   "--------\r\n" +
			                                   "(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			                                   "1. Mickey Mouse\r\n" +
			                                   "   Donald Duck\r\n" +
			                                   "2. Dagobert Duck\r\n" +
			                                   "   Daisy Duck\r\n" +
			                                   "\r\n" +
			                                   "Election2\r\n" +
			                                   "--------\r\n" +
			                                   "(J=Ja, E=Enthaltung, N=Nein)\r\n" +
			                                   "1. [X] One\r\n" +
			                                   "2. [E] Two\r\n" +
			                                   "3. [N] Three\r\n" +
			                                   "4. [E] Four\r\n" +
			                                   "\r\n" +
			                                   "\r\n" +
			                                   "12345\r\n");

			var output = Program.TallyBallots(_configFileName, _ballotDirectoryName);

			Assert.That(output, Is.EqualTo(@"The election
============

Total of 3 ballots, thereof 1 at least partially invalid.

Election1
---------
1. Dagobert Duck (4 points)
2. Mickey Mouse (3 points)
   Donald Duck (2 points)
   Daisy Duck (0 points)
(3 ballots, thereof 0 invalid)

Election2
---------
One: 1 J, 1 N, 0 E
Two: 1 J, 1 N, 0 E
Three: 1 J, 0 N, 1 E
Four: 0 J, 0 N, 2 E
(3 ballots, thereof 1 invalid)

"));
		}

	}
}