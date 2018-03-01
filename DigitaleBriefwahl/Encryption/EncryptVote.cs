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

				var guid = Guid.NewGuid().ToString();
				_FileName = $"{GetSanitizedElection(_Election)}_{guid.Replace("-", "")}.txt";

				return _FileName;
			}
		}

		public string BallotFilePath => Path.Combine(Path.GetTempPath(), FileName);

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

				using (var keyFile = File.OpenRead(publicKeyFile))
				{
					using (var inputStream = PgpUtilities.GetDecoderStream(keyFile))
					{

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
			}
		}

		private static byte[] CompressData(byte[] inputData)
		{
			using (var stream = new MemoryStream())
			{
				var compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);

				using (var compressedStream = compressedDataGenerator.Open(stream))
				{
					var lData = new PgpLiteralDataGenerator();

					// we want to Generate compressed data. This might be a user option later,
					// in which case we would pass in bOut.
					using (var pOut = lData.Open(
						compressedStream, // the compressed output stream
						PgpLiteralData.Binary,
						"data", // "filename" to store
						inputData.Length, // length of clear data
						DateTime.UtcNow // current time
					))
					{
						pOut.Write(inputData, 0, inputData.Length);

						lData.Close();
					}
				}
				compressedDataGenerator.Close();
				return stream.ToArray();
			}
		}

		private static byte[] EncryptData(byte[] inputData)
		{
			var bytes = CompressData(inputData);

			var encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256,
				true);

			encryptedDataGenerator.AddMethod(PublicKey);

			using (var encOut = new MemoryStream())
			{
				using (var cOut = encryptedDataGenerator.Open(encOut, bytes.Length))
				{
					cOut.Write(bytes, 0, bytes.Length); // obtain the actual bytes from the compressed stream
				}

				return encOut.ToArray();
			}
		}

		private static string GetSanitizedElection(string election)
		{
			return election.Replace(' ', '_').Replace('.', '_');
		}

		public string WriteVote(string vote)
		{
			var outputFileName = Path.ChangeExtension(BallotFilePath, ".gpg");
			var voteBytes = Encoding.UTF8.GetBytes(vote);

			using (var outputStream = new FileStream(outputFileName, FileMode.Create))
			{
				var outBytes = EncryptData(voteBytes);
				outputStream.Write(outBytes, 0, outBytes.Length);
			}
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
