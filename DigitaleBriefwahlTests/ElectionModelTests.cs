// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.IO;
using DigitaleBriefwahl;
using DigitaleBriefwahl.Model;
using IniParser;
using IniParser.Model;
using NUnit.Framework;

namespace DigitaleBriefwahlTests
{
	[TestFixture]
	public class ElectionModelTests
	{
		private IniData ReadIniDataFromString(string s)
		{
			var parser = new FileIniDataParser();
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.Write(s);
				writer.Flush();
				stream.Position = 0;
				var reader = new StreamReader(stream);
				return parser.ReadData(reader);
			}
		}

		[Test]
		public void ValidConfiguration()
		{
			// Setup
			string ini = @"[Election]
Text=Some description
ZwischentextVor2=Some text
Typ=Punktesystem
Stimmen=2
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
LimitKandidat2=2
Kandidat3=Dagobert Duck
";
			var data = ReadIniDataFromString(ini);

			// Execute
			var model = new ElectionModel("Election", data);

			// Verify
			Assert.That(model.Description, Is.EqualTo("Some description"));
			Assert.That(model.Type, Is.EqualTo(ElectionType.Weighted));
			Assert.That(model.Votes, Is.EqualTo(2));
			Assert.That(model.TextBefore, Is.EqualTo(new[] { null, "Some text" }));
			Assert.That(model.Nominees, Is.EqualTo(new[]
					{
						"Mickey Mouse",
						"Donald Duck",
						"Dagobert Duck"
					}));
			Assert.That(model.NomineeLimits.Count, Is.EqualTo(1));
			Assert.That(model.NomineeLimits["Donald Duck"], Is.EqualTo(new Tuple<int, int>(2, 2)));
		}

		[Test]
		public void Votes_Missing_ShouldThrow()
		{
			// Setup
			string ini = @"[Election]
Kandidat1=Donald Duck
";
			var data = ReadIniDataFromString(ini);

			// Execute/Verify
			Assert.That(() => new ElectionModel("Election", data),
				Throws.TypeOf<InvalidConfigurationException>().With.Message.StartsWith("Missing votes key ('Stimmen=')"));
		}

		[Test]
		public void Type_Missing_DefaultsToWeighted()
		{
			// Setup
			string ini = @"[Election]
Stimmen=1
Kandidat1=Donald Duck
";
			var data = ReadIniDataFromString(ini);

			// Execute
			var model = new ElectionModel("Election", data);

			// Verify
			Assert.That(model.Type, Is.EqualTo(ElectionType.Weighted));
			Assert.That(model.Votes, Is.EqualTo(1));
			Assert.That(model.Nominees, Is.EqualTo(new[] { "Donald Duck", }));
		}

		[Test]
		public void NomineeLimit_InvalidNomineeNumber_ShouldThrow()
		{
			// Setup
			string ini = @"[Election]
Stimmen=1
Kandidat1=Donald Duck
LimitKandidatX=2
";
			var data = ReadIniDataFromString(ini);

			// Execute/Verify
			Assert.That(() => new ElectionModel("Election", data),
				Throws.TypeOf<InvalidConfigurationException>().With.Message.StartsWith("Invalid nominee limit key:"));
		}

		[TestCase("x", Description = "SingleVote_NoNumber")]
		[TestCase("1-x", Description = "RangeVote_NoNumber")]
		[TestCase("0", Description = "SingleVote_NumberToSmall")]
		[TestCase("5", Description = "SingleVote_NumberToBig")]
		[TestCase("0,1", Description = "RangeVote_NumberToSmall")]
		[TestCase("0-1", Description = "RangeVote_NumberToSmall")]
		[TestCase("1-0", Description = "ReverseRangeVote_NumberToSmall")]
		[TestCase("1-5", Description = "RangeVote_NumberToBig")]
		[TestCase("0-1-2", Description = "MultipleRanges")]
		[TestCase("0-1,2", Description = "MultipleRangesWithComma")]
		public void NomineeLimit_ShouldThrow(string limit)
		{
			// Setup
			string ini = string.Format(@"[Election]
Stimmen=1
Kandidat1=Donald Duck
LimitKandidat1={0}
", limit);
			var data = ReadIniDataFromString(ini);

			// Execute/Verify
			Assert.That(() => new ElectionModel("Election", data),
				Throws.TypeOf<InvalidConfigurationException>().With.Message.StartsWith("Invalid nominee limit value:"));
		}

		[Test]
		public void NomineeLimit_Valid()
		{
			// Setup
			string ini = @"[Election]
Stimmen=1
Kandidat1=Donald Duck
LimitKandidat1=1-1
";
			var data = ReadIniDataFromString(ini);

			// Execute
			var model = new ElectionModel("Election", data);

			// Verify
			Assert.That(model.Type, Is.EqualTo(ElectionType.Weighted));
			Assert.That(model.Votes, Is.EqualTo(1));
			Assert.That(model.Nominees, Is.EqualTo(new[] { "Donald Duck", }));
		}

		[TestCase("x", Description = "NaN")]
		[TestCase("0", Description = "ToSmall")]
		[TestCase("3", Description = "ToBig")]
		public void TextBefore_ShouldThrow(string number)
		{
			// Setup
			string ini = string.Format(@"[Election]
ZwischentextVor{0}=Some text
Stimmen=2
", number);
			var data = ReadIniDataFromString(ini);

			// Execute/Verify
			Assert.That(() => new ElectionModel("Election", data),
				Throws.TypeOf<InvalidConfigurationException>().With.Message.StartsWith("Invalid TextBefore key:"));
		}
	}
}

