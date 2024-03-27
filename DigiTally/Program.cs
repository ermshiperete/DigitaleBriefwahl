using System;
using System.IO;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Model;
using DigitaleBriefwahl.Tally;

namespace DigiTally
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "DigiTally", args);

			// Setup command line options
			var configName = args.Length > 0 ? args[0] : Configuration.ConfigName;
			if (!File.Exists(configName))
			{
				Console.WriteLine($"Can't find {configName}. Exiting.");
				return;
			}

			var readBallot = new ReadBallots(configName);

			// foreach file in directory
			//     readBallot.AddBallot(file)

			// readBallot.Results
		}
	}
}