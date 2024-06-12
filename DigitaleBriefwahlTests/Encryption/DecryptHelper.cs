using System.IO;
using System.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities.IO;

namespace DigitaleBriefwahlTests.Encryption
{
	public static class DecryptHelper
	{
		private static PgpPrivateKey GetPrivateKey(string privateKeyPath, char[] passPhrase)
		{
			using Stream keyIn = File.OpenRead(privateKeyPath);
			using Stream inputStream = PgpUtilities.GetDecoderStream(keyIn);
			PgpSecretKeyRingBundle secretKeyRingBundle = new PgpSecretKeyRingBundle(inputStream);

			foreach (var kRing in secretKeyRingBundle.GetKeyRings())
			{
				foreach (var secretKey in kRing.GetSecretKeys())
				{
					return secretKey.ExtractPrivateKey(passPhrase);
				}
			}

			return null;
		}

		public static string DecryptFile(string inputFile, string privateKeyFile)
		{
			using var stream = File.OpenRead(inputFile);
			var outputFile = Path.GetTempFileName();
			if (DecryptFile(stream, outputFile, privateKeyFile))
			{
				var content = File.ReadAllText(outputFile);
				File.Delete(outputFile);
				return content;
			}

			File.Delete(outputFile);
			return null;
		}

		private static bool DecryptFile(Stream inputStream, string outputFile,
			string privateKeyLoc)
		{
			PgpEncryptedDataList enc;

			using var newStream = PgpUtilities.GetDecoderStream(inputStream);
			var pgpObjF = new PgpObjectFactory(newStream);
			if (pgpObjF.NextPgpObject() is PgpEncryptedDataList list)
			{
				enc = list;
			}
			else
			{
				enc = (PgpEncryptedDataList)pgpObjF.NextPgpObject();
			}

			var privKey = GetPrivateKey(privateKeyLoc, null);

			var pbe = enc.GetEncryptedDataObjects().Cast<PgpPublicKeyEncryptedData>().First();

			using var clear = pbe.GetDataStream(privKey);
			using var memoryStream = new MemoryStream();
			clear.CopyTo(memoryStream);
			var plainBytes = memoryStream.ToArray();
			var plainFact = new PgpObjectFactory(plainBytes);
			var message = plainFact.NextPgpObject();
			if (message is PgpCompressedData cData)
			{
				using var compDataIn = cData.GetDataStream();
				var o = new PgpObjectFactory(compDataIn);
				message = o.NextPgpObject();
				if (message is PgpOnePassSignatureList)
				{
					message = o.NextPgpObject();
				}

				PgpLiteralData literalData = null;
				literalData = (PgpLiteralData)message;
				if (literalData != null)
				{
					using var output = File.Create(outputFile);
					using var unc = literalData.GetInputStream();
					Streams.PipeAll(unc, output);
					return true;
				}
			}

			return false;
		}
	}
}