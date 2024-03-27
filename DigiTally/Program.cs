// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using System.Text;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Tally
{
	public class Program
	{
		public static string TallyBallots(string configName, string ballotsDirectory)
		{
			var bldr = new StringBuilder();
			var configuration = Configuration.Configure(configName);
			var readBallot = new ReadBallots(configName);

			foreach (var filename in Directory.GetFiles(ballotsDirectory))
			{
				readBallot.AddBallot(filename);
			}

			bldr.AppendLine(configuration.Title);
			bldr.AppendLine(new string('=', configuration.Title.Length));
			bldr.AppendLine();
			bldr.AppendLine(
				$"Total of {readBallot.NumberOfBallots} ballots, thereof {readBallot.NumberOfInvalidBallots} at least partially invalid.");
			bldr.AppendLine();
			bldr.Append(readBallot.GetResultString());
			return bldr.ToString();
		}
		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "DigiTally", args);

			// Setup command line options
			if (args.Length <= 0 || args[0] == "--help")
			{
				Console.WriteLine("Usage:");
				Console.WriteLine("DigiTally.exe ballotsDirectory [configFile]");
				Console.WriteLine("ballotsDirectory: directory that contains the decrypted ballots");
				Console.WriteLine("configFile: optional name and path of the config file");
				return;
			}
			var configName = args.Length > 1 ? args[1] : Configuration.ConfigName;
			if (!File.Exists(configName))
			{
				Console.WriteLine($"Can't find {configName}. Exiting.");
				return;
			}

			Console.WriteLine(TallyBallots(configName, args[0]));
		}
	}
}