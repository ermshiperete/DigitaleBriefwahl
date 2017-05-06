// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.IO;
using System.Text;
using DigitaleBriefwahl.Model;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace DigitaleBriefwahl.Encryption
{
	public class EncryptVote
	{
		public EncryptVote()
		{

		}

		private string FileName
		{
			get
			{
				var guid = Guid.NewGuid().ToString();
				return guid.Replace("-", "");
			}
		}

		private static PgpPublicKey PublicKey
		{
			get
			{
				using (var keyFile = File.OpenRead(Configuration.Current.PublicKey))
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
				PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(
					CompressionAlgorithmTag.Zip);

				using (var compressedStream = compressedDataGenerator.Open(stream))
				{
					PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();

					// we want to Generate compressed data. This might be a user option later,
					// in which case we would pass in bOut.
					using (Stream pOut = lData.Open(
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

		public string WriteVote(string election, string vote)
		{
			var outputFileName = Path.Combine(Path.GetTempPath(),
				$"{GetSanitizedElection(election)}_{FileName}.pgp");
			var voteBytes = Encoding.UTF8.GetBytes(vote);

			using (var outputStream = new FileStream(outputFileName, FileMode.Create))
			{
				var outBytes = EncryptData(voteBytes);
				outputStream.Write(outBytes, 0, outBytes.Length);
			}
			return outputFileName;
		}

		public string WriteVoteUnencrypted(string election, string vote)
		{
			var fileName = Path.Combine(Path.GetTempPath(),
				$"{GetSanitizedElection(election)}_{FileName}.txt");
			File.WriteAllText(fileName, vote);
			return fileName;
		}
	}
}
