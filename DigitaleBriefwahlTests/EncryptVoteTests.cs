// Copyright (c) 2021 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.IO;
using DigitaleBriefwahl;
using DigitaleBriefwahl.Encryption;
using NUnit.Framework;

namespace DigitaleBriefwahlTests
{
	[TestFixture]
	public class EncryptVoteTests
	{
		[Test]
		[Repeat(10)]
		public void BallotFilePath_HasConstantLength()
		{
			var sut = new EncryptVote("foo");
			var ballotFilePath = sut.BallotFilePath;
			var tempPath = Path.GetTempPath();
			tempPath = tempPath.Replace(@"\", @"\\");
			Assert.That(ballotFilePath, Does.Match($"{tempPath}foo_[0-9A-F]{{32}}.txt"));
			Assert.That(ballotFilePath.Length, Is.EqualTo(Path.GetTempPath().Length + 4 /* foo_ */ + 32 + 4 /* .txt */));
		}

		[Test]
		public void BallotFilePath_NumberMatchesBallotId()
		{
			var ballotId = BallotHelper.BallotId;
			var expectedNumber = ballotId.Replace("-", "");

			var sut = new EncryptVote("foo");
			Assert.That(sut.BallotFilePath, Is.EqualTo($"{Path.GetTempPath()}foo_{expectedNumber}.txt"));
		}

		[Test]
		public void GetEncryptedFilePath()
		{
			var sut = new EncryptVote("foo");
			Assert.That(sut.GetEncryptedFilePath(sut.BallotFilePath),
				Is.EqualTo(sut.BallotFilePath.Substring(0, sut.BallotFilePath.Length - 4) + ".gpg"));
		}
	}
}