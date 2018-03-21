// Copyright (c) 2017-2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using DigitaleBriefwahl;
using DigitaleBriefwahl.Encryption;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Model;
using Microsoft.Win32;
using SIL.Email;

namespace Packer
{
	internal static class Program
	{
		private static Configuration Config { get; set; }
		private static string ExecutableLocation => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "Packer");
			CreateBundle();

			if (Debugger.IsAttached)
				return;

			Console.WriteLine("Press 'Enter' to continue");
			Console.ReadLine();
		}

		private static bool CreateBundle()
		{
			PackCompiler.ConvertToUtf8(Configuration.ConfigName);
			Config = Configuration.Configure(Path.Combine(ExecutableLocation, Configuration.ConfigName));
			var packCompiler = new PackCompiler(ExecutableLocation);
			if (!File.Exists(packCompiler.ConfigFilename))
			{
				Console.WriteLine();
				Console.WriteLine($"The configuration file {packCompiler.ConfigFilename} is missing. Exiting.");
				return false;
			}

			if (!File.Exists(packCompiler.Config.PublicKey))
			{
				Console.WriteLine();
				Console.WriteLine(
					$"The public key file '{packCompiler.Config.PublicKey}' specified in {packCompiler.ConfigFilename} is missing. Exiting.");
				return false;
			}

			var zipFile = packCompiler.PackAllFiles();
			var ballotFile = WriteBallot();
			var publicKeyFile = WritePublicKey();
			Console.WriteLine($"The following files were created in {Path.GetDirectoryName(zipFile)}:");
			Console.WriteLine($"\t{Path.GetFileName(zipFile)}");
			Console.WriteLine($"\t{Path.GetFileName(ballotFile)}");
			Console.WriteLine($"\t{Path.GetFileName(publicKeyFile)}");

			if (CanUsePreferredEmailProvider)
				SendEmail(EmailProviderFactory.PreferredEmailProvider(), zipFile, ballotFile, publicKeyFile);
			return true;
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

		private static string GetDefaultValue(string path)
		{
			using (var key = Registry.CurrentUser.OpenSubKey(path))
			{
				return key?.GetValue("") as string;
			}
		}

		private static bool CanUsePreferredEmailProvider
		{
			get
			{
				if (!SIL.PlatformUtilities.Platform.IsWindows)
					return true;

				var retVal = !string.IsNullOrEmpty(GetDefaultValue(@"Software\Clients\Mail"));
				Logger.Log($"Can use perferred email provider: {retVal}");
				return retVal;
			}
		}

		private static bool SendEmail(IEmailProvider emailProvider, string zipFile, string ballotFile,
			string publicKeyFile)
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