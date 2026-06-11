// Copyright (c) 2024 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Model;

namespace DigitaleBriefwahl.Tally
{
	public static class Program
	{
		private static DigiTallyOptions Options { get; set; }


		private static ParserResult<DigiTallyOptions> GetParserResult(string[] args)
		{
			try
			{
				using var writer = new StreamWriter(Logger.LogFile, true);
				return DigiTallyOptions.ParseCommandLineArgs(writer, args);
			}
			catch (IOException)
			{
				// just ignore - we can't write to log file
				return DigiTallyOptions.ParseCommandLineArgs(null, args);;
			}
		}

		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "DigiTally", args);

			var parseResult = RetryUtils.Retry(() => GetParserResult(args));

			parseResult.WithNotParsed(x =>
			{
				var errors = x as Error[] ?? x.ToArray();
				var helpText = "";
				if (errors.IsVersion())
				{
					helpText = HelpText.AutoBuild(parseResult);
				}
				else
				{
					if (!errors.IsHelp())
					{
						Console.WriteLine("Kann übergebene Parameter nicht interpretieren.");
						Logger.Log("Can't parse arguments");
					}

					helpText = HelpText.AutoBuild(parseResult, h =>
					{
						h.AdditionalNewLineAfterOption = false;
						return HelpText.DefaultParsingErrorsHandler(parseResult, h);
					}, e => e);
				}

				Console.WriteLine(helpText);
				Logger.Log(helpText);
			});
			if (parseResult.Errors.Any())
			{
				return;
			}

			Options = parseResult.Value;
			var configName = Options.ConfigFile ?? Configuration.ConfigName;
			if (!File.Exists(configName))
			{
				Console.WriteLine($"Can't find {configName}. Exiting.");
				return;
			}

			var processor = new BallotProcessor(Options);
			Console.WriteLine(processor.GetElectionResults());
		}
	}
}