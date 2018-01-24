// Copyright (c) 2017-2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Model;

namespace Packer
{
	internal static class Program
	{
		private static Configuration Config { get; set; }
		private static string ExecutableLocation => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "Packer");
			Config = Configuration.Configure(Path.Combine(ExecutableLocation, Configuration.ConfigName));
			var zipFile = new PackCompiler(ExecutableLocation).PackAllFiles();
			var ballotFile = WriteBallot();
			var publicKeyFile = WritePublicKey();
			Console.WriteLine($"The following files were created in {Path.GetDirectoryName(zipFile)}:");
			Console.WriteLine($"\t{Path.GetFileName(zipFile)}");
			Console.WriteLine($"\t{Path.GetFileName(ballotFile)}");
			Console.WriteLine($"\t{Path.GetFileName(publicKeyFile)}");

			if (Debugger.IsAttached)
				return;

			Console.WriteLine("Press 'Enter' to continue");
			Console.ReadLine();
		}

		private static string MoveToExeLocation(string fileName)
		{
			var newFileName = Path.Combine(ExecutableLocation, $"ManuelleVerschluesselung_{Path.GetFileName(fileName)}");
			File.Copy(fileName, newFileName, true);
			return newFileName;
		}

		private static string WriteBallot()
		{
			var bldr = new StringBuilder();
			bldr.AppendLine(Config.Title);
			bldr.Append('=', Config.Title.Length);
			bldr.AppendLine();
			bldr.AppendLine();
			foreach (var election in Config.Elections)
			{
				bldr.AppendLine(election.GetResult(election.EmptyVotes, true));
			}
			var ballot = bldr.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n").ToString();
			return MoveToExeLocation(
				new DigitaleBriefwahl.Encryption.EncryptVote().WriteVoteUnencrypted(Config.Title, ballot));
		}

		private static string WritePublicKey()
		{
			return MoveToExeLocation(DigitaleBriefwahl.Encryption.EncryptVote.WritePublicKey(Config.Title));
		}
	}
}