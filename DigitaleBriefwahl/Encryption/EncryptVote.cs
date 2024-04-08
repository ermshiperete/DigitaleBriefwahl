// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.IO;
using System.Reflection;
using System.Text;
using DigitaleBriefwahl.Model;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace DigitaleBriefwahl.Encryption
{
	public class EncryptVote
	{
		private string _FileName;
		private readonly string _Election;

		public EncryptVote(string election)
		{
			_Election = election;
		}

		private string FileName
		{
			get
			{
				if (!string.IsNullOrEmpty(_FileName))
					return _FileName;

				_FileName = $"{GetSanitizedElection(_Election)}_{BallotHelper.BallotId.Replace("-", "")}.txt";

				return _FileName;
			}
		}

		public string BallotFilePath => Path.Combine(Path.GetTempPath(), FileName);

		public string GetEncryptedFilePath(string filePath)
		{
			return Path.ChangeExtension(filePath, ".gpg");
		}

		private string PublicKeyFileName =>
			$"{GetSanitizedElection(_Election)}_{Configuration.Current.PublicKey}";

		public string PublicKeyFilePath => Path.Combine(Path.GetTempPath(), PublicKeyFileName);

		private static PgpPublicKey PublicKey
		{
			get
			{
				var publicKeyFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					Configuration.Current.PublicKey);
				if (!File.Exists(publicKeyFile))
					throw new InvalidConfigurationException($"Can't find file for public key: '{publicKeyFile}'.");

				using var keyFile = File.OpenRead(publicKeyFile);
				using var inputStream = PgpUtilities.GetDecoderStream(keyFile);
				var publicKeyRingBundle = new PgpPublicKeyRingBundle(inputStream);

				foreach (PgpPublicKeyRing keyRing in publicKeyRingBundle.GetKeyRings())
				{
					foreach (PgpPublicKey key in keyRing.GetPublicKeys())
					{
						if (key.IsEncryptionKey)
						{
							return key;
						}
					}
				}

				throw new ArgumentException("Can't find encryption key in key ring.");
			}
		}

		private static byte[] CompressData(byte[] inputData)
		{
			using var stream = new MemoryStream();
			var compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);

			using var compressedStream = compressedDataGenerator.Open(stream);
			var lData = new PgpLiteralDataGenerator();

			// we want to Generate compressed data. This might be a user option later,
			// in which case we would pass in bOut.
			using (var pOut = lData.Open(
						compressedStream, // the compressed output stream
						PgpLiteralData.Binary,
						"data",           // "filename" to store
						inputData.Length, // length of clear data
						DateTime.UtcNow   // current time
						))
			{
				pOut.Write(inputData, 0, inputData.Length);
			}
			return stream.ToArray();
		}

		private static byte[] EncryptData(byte[] inputData)
		{
			var bytes = CompressData(inputData);

			var encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256,
				true);

			encryptedDataGenerator.AddMethod(PublicKey);

			using var encOut = new MemoryStream();
			using (var cOut = encryptedDataGenerator.Open(encOut, bytes.Length))
			{
				cOut.Write(bytes, 0, bytes.Length); // obtain the actual bytes from the compressed stream
			}

			return encOut.ToArray();
		}

		private const  string   Umlauts            = "äöüÄÖÜß";
		private static string[] UmlautReplacements = { "ae", "oe", "ue", "AE", "OE", "UE", "ss" };

		private static string GetSanitizedElection(string election)
		{
			var filePath = election.Replace(' ', '_').Replace('.', '_');
			var bldr = new StringBuilder();
			for (int i = 0; i < filePath.Length; i++)
			{
				var c = filePath[i];
				if (c > 128)
				{
					var umlautIndex = Umlauts.IndexOf(c);
					bldr.Append(umlautIndex >= 0 ? UmlautReplacements[umlautIndex] : "_");
				}
				else
					bldr.Append(c);
			}

			return bldr.ToString();
		}

		public string WriteVote(string vote, string filePath = null)
		{
			if (string.IsNullOrEmpty(filePath))
				filePath = BallotFilePath;
			if (Path.GetFileName(filePath) != FileName)
				filePath = Path.Combine(Path.GetDirectoryName(filePath), FileName);
			var outputFileName = GetEncryptedFilePath(filePath);
			var voteBytes = Encoding.UTF8.GetBytes(vote);

			using var outputStream = new FileStream(outputFileName, FileMode.Create);
			var outBytes = EncryptData(voteBytes);
			outputStream.Write(outBytes, 0, outBytes.Length);
			return outputFileName;
		}

		public string WriteVoteUnencrypted(string vote, string filePath = null)
		{
			if (string.IsNullOrEmpty(filePath))
				filePath = BallotFilePath;
			if (Path.GetFileName(filePath) != FileName)
				filePath = Path.Combine(Path.GetDirectoryName(filePath), FileName);
			File.WriteAllText(filePath, vote);
			return filePath;
		}

		public string WritePublicKey(string filePath = null)
		{
			if (string.IsNullOrEmpty(filePath))
				filePath = PublicKeyFilePath;
			File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				Configuration.Current.PublicKey), filePath, true);
			return filePath;
		}

	}
}
