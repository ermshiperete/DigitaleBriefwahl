// Copyright (c) 2021 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using DigitaleBriefwahl;
using NUnit.Framework;

namespace DigitaleBriefwahlTests
{
	[TestFixture]
	public class BallotHelperTests
	{
		[Test]
		public void GetBallot()
		{
			var electionResults = new List<string>();
			electionResults.Add(@"Geschäftsführer
---------------
(J=Ja, E=Enthaltung, N=Nein)
1. [J] Siegfried Siegreich
");
			electionResults.Add(@"Vorstand
--------
(1 Stimme; Wahl mit 1. kennzeichnen)
   Max Mayer
1. Anton Anders
   Gustav Graf
");

			Assert.That(BallotHelper.GetBallot("Vereinswahlen 2017", electionResults),
				Is.EqualTo("Vereinswahlen 2017\r\n" +
							"==================\r\n" +
							"\r\n" +
							"Geschäftsführer\r\n" +
							"---------------\r\n" +
							"(J=Ja, E=Enthaltung, N=Nein)\r\n" +
							"1. [J] Siegfried Siegreich\r\n" +
							"\r\n" +
							"Vorstand\r\n" +
							"--------\r\n" +
							"(1 Stimme; Wahl mit 1. kennzeichnen)\r\n" +
							"   Max Mayer\r\n" +
							"1. Anton Anders\r\n" +
							"   Gustav Graf\r\n" +
							"\r\n" +
							"\r\n" +
							$"{BallotHelper.BallotId}\r\n"));
		}
	}
}