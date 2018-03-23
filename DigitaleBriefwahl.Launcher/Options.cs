// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.IO;
using CommandLine;
using CommandLine.Text;

namespace DigitaleBriefwahl.Launcher
{
	internal class Options
	{
		[Option("run", MutuallyExclusiveSet = "run")]
		public string RunApp { get; set; }

		[Option("rundir", MutuallyExclusiveSet = "run")]
		public string RunDirectory { get; set; }

		[Option("url", MutuallyExclusiveSet = "run")]
		public string UrlFile { get; set; }

		[Option("no-check")]
		public bool SkipUpdateCheck { get; set; }

		[Option("squirrel-install")]
		public string Install { get; set; }

		[Option("squirrel-updated")]
		public string Updated { get; set; }

		[Option("squirrel-obsolete")]
		public string Obsolete { get; set; }

		[Option("squirrel-uninstall")]
		public string Uninstall { get; set; }

		[Option("squirrel-firstrun")]
		public bool FirstRun { get; set; }

		[HelpOption('h', "help")]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}

		[ParserState]
		public IParserState LastParserState { get; set; }

		public static Options ParseCommandLineArgs(TextWriter writer, string[] args)
		{
			var options = new Options();
			using (var parser = new Parser(s =>
			{
				s.MutuallyExclusive = true;
				s.HelpWriter = writer;
			}))
			{
				return parser.ParseArguments(args, options) ? options : null;
			}
		}

		public string PackageDir { get; set; }

		public bool IsInstall => !string.IsNullOrEmpty(Install);
		public bool IsUpdated => !string.IsNullOrEmpty(Updated);
		public bool IsObsolete => !string.IsNullOrEmpty(Obsolete);
		public bool IsUninstall => !string.IsNullOrEmpty(Uninstall);

		public bool IsSquirrelCommand => IsInstall ||
										IsUpdated ||
										IsObsolete ||
										IsUninstall ||
										FirstRun;
	}
}
