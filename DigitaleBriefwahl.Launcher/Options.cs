// Copyright (c) 2018-2021 Eberhard Beilharz
// This software is licensed under the GNU General public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.IO;
using CommandLine;

namespace DigitaleBriefwahl.Launcher
{
	internal class Options
	{
		[Option("run", MetaValue = "ZIP", SetName = "run",
			HelpText = "Name and path of the zipped election application")]
		public string RunApp { get; set; }

		[Option("rundir", MetaValue = "DIR", SetName = "rundir",
			HelpText = "Directory with the unzipped election application")]
		public string RunDirectory { get; set; }

		[Option("url", MetaValue = "URL", SetName = "url",
			HelpText = "Name and path of a .wahlurl file that contains URL of election")]
		public string UrlFile { get; set; }

		[Option("no-check", HelpText = "Skip check for available updates.")]
		public bool SkipUpdateCheck { get; set; }

		[Option("squirrel-install", Hidden = true)]
		public string Install { get; set; }

		[Option("squirrel-updated", Hidden = true)]
		public string Updated { get; set; }

		[Option("squirrel-obsolete", Hidden = true)]
		public string Obsolete { get; set; }

		[Option("squirrel-uninstall", Hidden = true)]
		public string Uninstall { get; set; }

		[Option("squirrel-firstrun", Hidden = true)]
		public bool FirstRun { get; set; }

		public static ParserResult<Options> ParseCommandLineArgs(TextWriter writer, string[] args)
		{
			using var parser = new Parser(s => s.HelpWriter = writer);
			return parser.ParseArguments<Options>(args);
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
