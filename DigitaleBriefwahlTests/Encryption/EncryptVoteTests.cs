// Copyright (c) 2021 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DigitaleBriefwahl;
using DigitaleBriefwahl.Encryption;
using DigitaleBriefwahl.Model;
using NUnit.Framework;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO;

namespace DigitaleBriefwahlTests.Encryption
{
	[TestFixture]
	public class EncryptVoteTests
	{
		private readonly List<string> _filesToDelete = new List<string>();

		[TearDown]
		public void TearDown()
		{
			foreach (var file in _filesToDelete)
			{
				try
				{
					if (Directory.Exists(file))
					{
						foreach (var f in Directory.GetFiles(file))
						{
							File.Delete(f);
						}

						Directory.Delete(file);
					}
					else
					{
						File.Delete(file);
					}
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
				}
			}
			_filesToDelete.Clear();
		}

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

		[Test]
		public void GetEncryptedFilePath_Umlaut_AllASCII()
		{
			var sut= new EncryptVote("ÄÖÜäöüß");
			var filePath = Path.GetFileName(sut.GetEncryptedFilePath(sut.BallotFilePath));
			Assert.That(filePath.StartsWith("AEOEUEaeoeuess"), Is.True,
				$"Unexpected FilePath.\n    Expected: AEOEUEaeoeuess...\n    But was:  {filePath}");
		}

		[Test]
		public void GetEncryptedFilePath_Other_AllASCII()
		{
			var sut= new EncryptVote("ខ្មែរ");
			var filePath = sut.GetEncryptedFilePath(sut.BallotFilePath);
			foreach (var c in filePath.ToCharArray())
			{
				Assert.That(c, Is.LessThanOrEqualTo(128),
					$"FilePath contains non-ASCII characters: {filePath}");
			}
		}

		#region Test Keys

		private const string KeyHandle = "484AE369B6634B18";
		private const string PublicKey = @"-----BEGIN PGP PUBLIC KEY BLOCK-----

mI0EZmc63wEEANYnSkt/s1S7Jxn9WdN/oZlDRw+NRcFb6DrTg9qil+IBoQ56Sj/s
HJmhOunZcNSQv3l1KzZ+Uy+JIOXCed0FGgoipc0TJUjCZa8I7v/Q30LPtlotSr3g
DjoFz9edDQ6vSEUiKaNrBMyqyBu76o66s8abfa36QxWK7CFnum8ztR6JABEBAAG0
DURpZ2l3YWhsIFRlc3SIzgQTAQoAOBYhBKGXuzRDmCQ0KgxA4EhK42m2Y0sYBQJm
ZzrfAhsDBQsJCAcDBRUKCQgLBRYCAwEAAh4BAheAAAoJEEhK42m2Y0sYbJID/1JW
5khjkcszWej1BnGfubdKL/5quIfzZ0V6b/UrTmWwSFNnlvXhFvGEyfptm5GuwXTF
TQqyoWlVTI0StWxTbhYECLDUdGtreKsNIDCNSxomDb8fB7wZx6V+n34Mmcls1DRy
Y7v3UJG7pvdOhOYiP5VrsTS/5sJ3/f1ZGQOwWHaduI0EZmc63wEEAMyV9mBNWfDc
L5xCZAIIHCr/aSkFswn2/hwfI5DXR2GpBJsaF5GD9mFZwmfHHRmHKy32mRIfJdH3
oEhZKSI74+wRTR5GpAlWzQRNV1vHJJYPF9tliDFl1DTHxLeWMkiW1J8GbZth94zP
7JRkk7YTswDMdeT/eAThj3Y+FK6VgMsBABEBAAGItgQYAQoAIBYhBKGXuzRDmCQ0
KgxA4EhK42m2Y0sYBQJmZzrfAhsMAAoJEEhK42m2Y0sYvtgD/23SSlQ0C835jCBf
JRhm6fwSnCk4V9KZUg5uIk8b+8P7FsiJpdl4GwaPK8bwYS3yNE/ilBTBR7s8eHWI
8TOZFChiOJZXU0UByL9MlSX6kos/kAEu8tVzcw2nwr9PAC7ILT6EcvLuPCqQ7j01
PZ+GJ7xGnHB/W53Im+DIMxdon6+l
=Iqyo
-----END PGP PUBLIC KEY BLOCK-----
";

		private const string PrivateKeyData = @"lQHYBGZnOt8BBADWJ0pLf7NUuycZ/VnTf6GZQ0cPjUXBW+g604PaopfiAaEOeko/7ByZoTrp2XDU
kL95dSs2flMviSDlwnndBRoKIqXNEyVIwmWvCO7/0N9Cz7ZaLUq94A46Bc/XnQ0Or0hFIimjawTM
qsgbu+qOurPGm32t+kMViuwhZ7pvM7UeiQARAQABAAP+IAvLG1clr8U9ya7W1UZhGT0vEg2y5Ydq
Tfs+3OW90q0pu6/jEoOzl+2/kiTetIBglr1I1SO8MP3VNWyPrdpIlftdNvo3b4F/P+To43pgk2+9
Dk7RP885AG+9auDGZNiqAxJgm40RlnleV2EDOYBlb/yNHWecqkAGr/F+p/oWnC0CANdfdtbMT4bm
PCdHA7xa+wmfP+ZbtzLQFuy16SvCgJMH9aybZijO2Opuq3bfQ8RE4pTNWN2USlp8gnn5TjuO+h8C
AP6M8FCynwun7PBOXDaoueQzgJt4KLrZBRXeRKHdAeET3M0XDRsGezSEH/cVGCytr72RxBzPvYTU
zMnR5rZaIlcB/2yDh4SNjvUYpecuLtEvcMMsCU40QEDBmrpZ+2xwsm6ohuSrI87o6YV13vbZoAZq
qFxWFglgG9w+cFFK+setVZamsrQNRGlnaXdhaGwgVGVzdIjOBBMBCgA4FiEEoZe7NEOYJDQqDEDg
SErjabZjSxgFAmZnOt8CGwMFCwkIBwMFFQoJCAsFFgIDAQACHgECF4AACgkQSErjabZjSxhskgP/
UlbmSGORyzNZ6PUGcZ+5t0ov/mq4h/NnRXpv9StOZbBIU2eW9eEW8YTJ+m2bka7BdMVNCrKhaVVM
jRK1bFNuFgQIsNR0a2t4qw0gMI1LGiYNvx8HvBnHpX6ffgyZyWzUNHJju/dQkbum906E5iI/lWux
NL/mwnf9/VkZA7BYdp2dAdgEZmc63wEEAMyV9mBNWfDcL5xCZAIIHCr/aSkFswn2/hwfI5DXR2Gp
BJsaF5GD9mFZwmfHHRmHKy32mRIfJdH3oEhZKSI74+wRTR5GpAlWzQRNV1vHJJYPF9tliDFl1DTH
xLeWMkiW1J8GbZth94zP7JRkk7YTswDMdeT/eAThj3Y+FK6VgMsBABEBAAEAA/0YNwPDYERuAtHL
z/vgVa9n03VR+q+3VTm8gF8ITMLRJ105Mnxpp9HoseQVvnaOR/ZvaUmy4GbslD+vcIYy3B3c4+1Z
iYsGgJMr9m02aFCXzpHLFVvb0t+hZ7RFpEav0J5Zmp23br/SUoby+QhKp5cW8xTte7jtawTKWj2j
DBz0wQIA2cM4nJ1ia/tJp+jNuFEni64OWm3J0OmL4zWWUB9o0fUPT5tRzyjB4Z+thMaRLp2IPGiz
yhkisQbDnmNyNHyoaQIA8IJsWcdJK6WWSnA15BZ0pXwpaYZqZU/m7SaANCR+kRZFqGOHkdeijq7v
hX93eIwY0SLsFHQh60m3gdNRYHR62QIA49xtsTi3bj1MXe00fhN6dm2pbXaVJdZrzKSVuZkMgBqP
u7r9CBhSzYE6jtp27v4nfBBq6uECgq7qIe0RnTugNqXtiLYEGAEKACAWIQShl7s0Q5gkNCoMQOBI
SuNptmNLGAUCZmc63wIbDAAKCRBISuNptmNLGL7YA/9t0kpUNAvN+YwgXyUYZun8EpwpOFfSmVIO
biJPG/vD+xbIiaXZeBsGjyvG8GEt8jRP4pQUwUe7PHh1iPEzmRQoYjiWV1NFAci/TJUl+pKLP5AB
LvLVc3MNp8K/TwAuyC0+hHLy7jwqkO49NT2fhie8Rpxwf1udyJvgyDMXaJ+vpQ==
";
		#endregion

		[Test]
		public void EncryptVote_CanDecrypt()
		{
			// Setup
			var privateKeyFile = Path.GetTempFileName();
			_filesToDelete.Add(privateKeyFile);
			var privateKey = Base64.Decode(PrivateKeyData);
			File.WriteAllBytes(privateKeyFile, privateKey);

			var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			_filesToDelete.Add(tempFolder);

			var publicKey = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				$"{KeyHandle}.asc");
			File.WriteAllText(publicKey, PublicKey);

			var wahlini = Path.Combine(tempFolder, "wahl.ini");
			File.WriteAllText(wahlini, @$"[Wahlen]
Titel=Test
PublicKey={KeyHandle}.asc
Email=nobody@example.com");

			var configuration = Configuration.Configure(wahlini);

			var vote = "Vereinswahlen 2024\r\n" +
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
				$"{BallotHelper.BallotId}\r\n";

			var voteFile = Path.Combine(tempFolder,
				$"Test_{BallotHelper.BallotId.Replace("-", "")}.txt");

			// Execute
			var filename = new EncryptVote("Test").WriteVote(vote, voteFile);
			_filesToDelete.Add(filename);

			// Verify
			Assert.That(filename, Is.EqualTo(Path.Combine(tempFolder, Path.GetFileNameWithoutExtension
				(voteFile) + ".gpg")));
			Assert.That(DecryptHelper.DecryptFile(filename, privateKeyFile), Is.EqualTo(vote));
		}
	}
}