// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DigitaleBriefwahl.Model;
using NUnit.Framework;

namespace DigitaleBriefwahlTests.Model
{
	[TestFixture]
	public class WeightedElectionModelTests
	{
		private const string BallotDagobertMickey = "Election\r\n" +
			"--------\r\n" +
			"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			"2. Mickey Mouse\r\n" +
			"   Donald Duck\r\n" +
			"1. Dagobert Duck\r\n";

		private const string BallotDonaldDagobert = "Election\r\n" +
			"--------\r\n" +
			"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			"   Mickey Mouse\r\n" +
			"1. Donald Duck\r\n" +
			"2. Dagobert Duck\r\n";

		private const string BallotDonaldOnly = "Election\r\n" +
			"--------\r\n" +
			"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			"   Mickey Mouse\r\n" +
			"1. Donald Duck\r\n" +
			"   Dagobert Duck\r\n";

		private const string BallotNone = "Election\r\n" +
			"--------\r\n" +
			"(2 Stimmen; Wahl der Reihenfolge nach mit 1.-2. kennzeichnen)\r\n" +
			"   Mickey Mouse\r\n" +
			"   Donald Duck\r\n" +
			"   Dagobert Duck\r\n";

		[TestCase("Dagobert Duck\nMickey Mouse", false, ExpectedResult = BallotDagobertMickey)]
		[TestCase("Donald Duck\nDagobert Duck", false, ExpectedResult = BallotDonaldDagobert)]
		[TestCase("", true, ExpectedResult = BallotNone)]
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
			var sut = ElectionModelFactory.Create("Election", data);

			// Exercise/Verify
			Assert.That(sut.EmptyVotes, Is.EqualTo(new[] { "Mickey Mouse", "Donald Duck"}));
		}

		[Test]
		public void GetInvalidVotes_AllOk()
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var sut = ElectionModelFactory.Create("Election", data);

			var result = sut.GetInvalidVotes(new List<string>(new[]
				{ "Mickey Mouse", "Donald Duck", "Dagobert Duck"}));

			Assert.That(result.Count, Is.EqualTo(0));
		}

		[Test]
		public void GetInvalidVotes_DuplicateName()
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var sut = ElectionModelFactory.Create("Election", data);

			var result = sut.GetInvalidVotes(new List<string>(new[]
				{ "Mickey Mouse", "Donald Duck", "Donald Duck"}));

			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result.First(), Is.EqualTo(1));
			Assert.That(result.Last(), Is.EqualTo(2));
		}

		[Test]
		public void GetInvalidVotes_InvalidName()
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var sut = ElectionModelFactory.Create("Election", data);

			var result = sut.GetInvalidVotes(new List<string>(new[]
				{ "Mickey Mouse", "X", "Donald Duck"}));

			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.First(), Is.EqualTo(1));
		}

		[TestCase(new [] { "Mickey Mouse", "Donald Duck"}, 2)]
		[TestCase(new [] { "Mickey Mouse", "Donald Duck", ""}, 2)]
		[TestCase(new [] { "", "Mickey Mouse", "Donald Duck", }, 0)]
		public void GetInvalidVotes_Incomplete(string[] votes, int expectedInvalid)
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=Weighted
Stimmen=3
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var sut = ElectionModelFactory.Create("Election", data);

			var result = sut.GetInvalidVotes(new List<string>(votes));

			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.First(), Is.EqualTo(expectedInvalid));
		}

		[TestCase(new [] { "Mickey Mouse", "Donald Duck"}, 2)]
		[TestCase(new [] { "Mickey Mouse", "Donald Duck", ""}, 2)]
		[TestCase(new [] { "", "Mickey Mouse", "Donald Duck", }, 0)]
		public void GetInvalidVotes_IncompleteOk(string[] votes, int expectedInvalid)
		{
			// Setup
			const string ini = @"[Election]
Text=Some description
Typ=Weighted
Stimmen=3
FehlendOk=true
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
Kandidat3=Dagobert Duck
";
			var data = ElectionModelTests.ReadIniDataFromString(ini);
			var sut = ElectionModelFactory.Create("Election", data);

			var result = sut.GetInvalidVotes(new List<string>(votes));

			Assert.That(result.Count, Is.EqualTo(0));
		}

		[TestCase(BallotDagobertMickey, new[] { "Mickey Mouse:0:1", "Donald Duck:0:0", "Dagobert Duck:0:2" })]
		[TestCase(BallotDonaldDagobert, new[] { "Mickey Mouse:0:0", "Donald Duck:0:2", "Dagobert Duck:0:1" })]
		[TestCase(BallotDonaldOnly,     new[] { "Mickey Mouse:0:0", "Donald Duck:0:2", "Dagobert Duck:0:0" })]
		[TestCase(BallotNone,           new[] { "Mickey Mouse:0:0", "Donald Duck:0:0", "Dagobert Duck:0:0" })]
		public void ReadVotesFromBallot(string ballot, string[] expectedResultStrings)
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
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ballot.Replace("\r", "")));
			using var reader = new StreamReader(stream);
			// skip first two lines of ballot
			reader.ReadLine();
			reader.ReadLine();

			// Execute
			var results = model.ReadVotesFromBallot(reader, null);

			// Verify
			var expectedResults = new Dictionary<string, CandidateResult>();
			foreach (var expectedString in expectedResultStrings)
			{
				var split = expectedString.Split(':');
				expectedResults.Add(split[0], new WeightedCandidateResult(int.Parse(split[1]),
					int.Parse(split[2])));
			}

			Assert.That(results, Is.EqualTo(expectedResults));
		}

		[TestCase(BallotDagobertMickey, ExpectedResult = @"1. Dagobert Duck (2 points)
2. Mickey Mouse (1 points)
   Donald Duck (0 points)
(1 ballots, thereof 0 invalid; max 2 points)
")]
		[TestCase(BallotDonaldDagobert, ExpectedResult = @"1. Donald Duck (2 points)
2. Dagobert Duck (1 points)
   Mickey Mouse (0 points)
(1 ballots, thereof 0 invalid; max 2 points)
")]
		[TestCase(BallotDonaldOnly, ExpectedResult = @"1. Donald Duck (2 points)
   Dagobert Duck (0 points)
   Mickey Mouse (0 points)
(1 ballots, thereof 0 invalid; max 2 points)
")]
		[TestCase(BallotNone, ExpectedResult =  @"   Dagobert Duck (0 points)
   Donald Duck (0 points)
   Mickey Mouse (0 points)
(1 ballots, thereof 0 invalid; max 2 points)
")]
		public string GetResultString(string ballot)
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
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ballot.Replace("\r", "")));
			using var reader = new StreamReader(stream);
			// skip first two lines of ballot
			reader.ReadLine();
			reader.ReadLine();
			var results = model.ReadVotesFromBallot(reader, null);

			// Execute
			return model.GetResultString(results);
		}

	}
}