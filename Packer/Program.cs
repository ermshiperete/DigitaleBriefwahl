// Copyright (c) 2017-2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitaleBriefwahl;
using DigitaleBriefwahl.Encryption;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Mail;
using DigitaleBriefwahl.Model;
using DigitaleBriefwahl.Utils;

namespace Packer
{
	internal static class Program
	{
		private static Configuration Config { get; set; }
		private static string Token { get; set; }
		private static bool UseGist { get; set; } = true;
		private static string ExecutableLocation => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "Packer", args);
			Logger.CreateLogger(new ConsoleLogger());

			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				switch (arg)
				{
					case "--token":
						if (i + 1 < args.Length)
						{
							Token = args[++i];
						}
						else
							Help();
						break;
					case "--no-gist":
						UseGist = false;
						break;
					default:
						Help();
						break;
				}
			}

			Task.WaitAll(CreateBundle(UseGist, Token));

			if (Debugger.IsAttached)
				return;

			Console.WriteLine("Press 'Enter' to finish");
			Console.ReadLine();
		}

		private static void Help()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("packer [--token <GithubToken>] [--no-gist]");
			Console.WriteLine();
			Console.WriteLine("--token <GitHubToken>\tGitHub token used to authenticate");
			Console.WriteLine("--no-gist\tManual upload to Google Drive or OneDrive instead of uploading to Gist");
			Environment.Exit(1);
		}

		private static async Task<bool> CreateBundle(bool uploadToGist, string token)
		{
			if (!File.Exists(Configuration.ConfigName))
			{
				await Console.Error.WriteLineAsync();
				await Console.Error.WriteLineAsync($"Config: The configuration file '{Configuration.ConfigName}' is missing. Exiting.");
				return false;
			}

			PackCompiler.ConvertToUtf8(Configuration.ConfigName);
			Config = Configuration.Configure(Path.Combine(ExecutableLocation, Configuration.ConfigName));
			var packCompiler = new PackCompiler(ExecutableLocation);
			if (!File.Exists(packCompiler.ConfigFilename))
			{
				await Console.Error.WriteLineAsync();
				await Console.Error.WriteLineAsync($"PackCompiler: The configuration file '{packCompiler.ConfigFilename}' is missing. Exiting.");
				return false;
			}

			if (!File.Exists(packCompiler.Config.PublicKey))
			{
				await Console.Error.WriteLineAsync();
				await Console.Error.WriteLineAsync(
					$"PackCompiler: The public key file '{packCompiler.Config.PublicKey}' specified in {packCompiler.ConfigFilename} is missing. Exiting.");
				return false;
			}

			var zipFile = packCompiler.PackAllFiles();
			var ballotFile = WriteBallot();
			var publicKeyFile = WritePublicKey();
			var zipFileHash = CalculateHash(zipFile);
			Console.WriteLine();
			Console.WriteLine("Files packed successfully.");
			string urlString = null;
			if (uploadToGist)
			{
				if (string.IsNullOrEmpty(token))
				{
					Console.Write("Enter the GitHub personal access token for authentication: ");
					token = Console.ReadLine();
				}

				Console.WriteLine("Uploading to GitHub Gist...");
				var gist = new GitHubGist();
				Task.WaitAll(gist.Initialize(token));
				urlString = await gist.Upload(packCompiler.Config.Title, zipFile);
				if (!string.IsNullOrEmpty(urlString))
					Console.WriteLine("Upload succeeded.");
			}
			else
			{
				Console.WriteLine(
					$"Now upload the file '{Path.GetFileName(zipFile)}' in directory");
				Console.WriteLine($"'{Path.GetDirectoryName(zipFile)}'");
				Console.WriteLine("to a suitable website.");
				Console.WriteLine("Then enter the full Download URL for that file:");
				for (var urlOk = false; !urlOk;)
				{
					Console.Write("> ");
					urlString = Console.ReadLine();
					try
					{
						urlString = AdjustUrlIfGoogleDrive(urlString);
						urlString = AdjustUrlIfMicrosoftOneDrive(urlString);
						var url = new Uri(urlString);
						urlOk = TryDownload(url, zipFileHash);
						if (urlOk)
							urlString = url.ToString();
					}
					catch (UriFormatException)
					{
						await Console.Error.WriteLineAsync(
							"Invalid URL. Please enter a valid download URL:");
					}
				}
			}

			if (string.IsNullOrEmpty(urlString))
				return false;

			var urlFile = Path.ChangeExtension(zipFile, "wahlurl");
			File.WriteAllText(urlFile, urlString);

			Console.WriteLine();
			Console.WriteLine($"The following files were created in {Path.GetDirectoryName(zipFile)}:");
			Console.WriteLine($"\t{Path.GetFileName(urlFile)}");
			Console.WriteLine($"\t{Path.GetFileName(ballotFile)}");
			Console.WriteLine($"\t{Path.GetFileName(publicKeyFile)}");

			var provider = EmailProviderFactory.GetPreferredEmailProvider(false);
			if (provider != null)
				SendEmail(provider, urlFile, ballotFile, publicKeyFile);
			else
				Console.WriteLine("Can't find an email provider; not sending email");
			return true;
		}

		private static bool TryDownload(Uri uri, byte[] originalHash)
		{
			var targetFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				Console.WriteLine("Verifying download URL:");
				Console.Write("    - downloading file: ");
				Downloader.DownloadFile(uri, targetFile).Wait();

				Console.WriteLine("    - comparing file content");
				if (!VerifyDownloadedFile(targetFile, originalHash))
				{
					Console.Error.WriteLine();
					Console.Error.WriteLine("Downloaded file doesn't match expected uploaded file. Please check that");
					Console.Error.WriteLine("the file can be downloaded without requiring authentication.");
					Console.WriteLine("Please enter a valid download URL:");
					return false;
				}

				Console.WriteLine("Download URL is valid.");
				return true;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine();
				Console.Error.WriteLine($"Error trying to download file: {e.Message}");
				Console.WriteLine("Please enter a valid download URL:");
				return false;
			}
			finally
			{
				try
				{
					File.Delete(targetFile);
				}
				catch
				{
				}
			}
		}

		private static string AdjustUrlIfGoogleDrive(string urlString)
		{
			if (!urlString.StartsWith("https://drive.google.com"))
				return urlString;

			Regex regex = null;
			if (urlString.StartsWith("https://drive.google.com/file"))
				regex = new Regex("https://drive.google.com/file/d/([^/]+)/");
			else if (urlString.StartsWith("https://drive.google.com/open"))
				regex = new Regex(@"https://drive.google.com/open\?id=(.+)");
			return regex != null && regex.IsMatch(urlString)
				? $"https://drive.google.com/uc?export=download&id={regex.Match(urlString).Groups[1]}"
				: urlString;
		}

		private static string AdjustUrlIfMicrosoftOneDrive(string urlString)
		{
			// <iframe src="https://onedrive.live.com/embed?cid=F2063761FAADDF22&resid=F2063761FAADDF22%21421&authkey=ALEvCse_3f7okbk" width="98" height="120" frameborder="0" scrolling="no"></iframe>
			if (!urlString.Contains("https://onedrive.live.com"))
				return urlString;

			var regex = new Regex("<iframe src=\"([^\"]+)\".+");
			if (regex.IsMatch(urlString))
				urlString = regex.Match(urlString).Groups[1].ToString();

			return urlString.Replace("/embed?", "/download?");
		}

		private static bool VerifyDownloadedFile(string fileName, byte[] originalHash)
		{
			var downloadedFileHash = CalculateHash(fileName);
			if (downloadedFileHash.Length != originalHash.Length)
				return false;

			return !downloadedFileHash.Where((hashByte, index) => hashByte != originalHash[index]).Any();
		}

		private static byte[] CalculateHash(string fileName)
		{
			using var sha256 = SHA256.Create();
			using var stream = File.OpenRead(fileName);
			return sha256.ComputeHash(stream);
		}

		private static string MoveToExeLocation(string fileName)
		{
			var newFileName = Path.Combine(ExecutableLocation, $"ManuelleVerschluesselung_{Path.GetFileName(fileName)}");
			File.Copy(fileName, newFileName, true);
			return newFileName;
		}

		private static string WriteBallot()
		{
			var results = new List<string>();
			foreach (var election in Config.Elections)
			{
				results.Add(election.GetResult(election.EmptyVotes, true));
			}

			var ballot = BallotHelper.GetBallot(Config.Title, results);
			return MoveToExeLocation(
				new EncryptVote(Config.Title).WriteVoteUnencrypted(ballot));
		}

		private static string WritePublicKey()
		{
			return MoveToExeLocation(new EncryptVote(Config.Title).WritePublicKey());
		}


		private static bool SendEmail(IEmailProvider emailProvider, string zipFile,
			string ballotFile, string publicKeyFile)
		{
			if (emailProvider == null)
				return false;

			var email = emailProvider.CreateMessage();

			email.Subject = Config.Title;
			email.AttachmentFilePath.Add(zipFile);
			email.AttachmentFilePath.Add(ballotFile);
			email.AttachmentFilePath.Add(publicKeyFile);

			return emailProvider.SendMessage(email);
		}
	}
}