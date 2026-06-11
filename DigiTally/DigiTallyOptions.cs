// Copyright (c) 2026 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.IO;
using CommandLine;

namespace DigitaleBriefwahl.Tally
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class DigiTallyOptions
	{
		[Option("config", MetaValue = "CONFIG", Required = false,
			HelpText = "Optional name and path of configuration file")]
		public string ConfigFile { get; set; }

		[Option('c', "CreateCsv", Required = false, Default = false,
			HelpText = "Create CSV file")]
		public bool CreateCsv { get; set; }

		[Option("CsvFile", MetaValue = "CSVFILE", Required = false, HelpText = "CSV file")]
		public string CsvFile { get; set; }

		[Option('p', "PrintBallots", Required = false, Default = false,
			HelpText = "Print ballots")]
		public bool PrintBallots { get; set; }

		[Value(0, MetaName = "ballotsDirectory", Required = true)]
		public string BallotsDirectory { get; set; }

		public static ParserResult<DigiTallyOptions> ParseCommandLineArgs(TextWriter writer, string[] args)
		{
			using var parser = new Parser(s => s.HelpWriter = writer);
			return parser.ParseArguments<DigiTallyOptions>(args);
		}
	}
}