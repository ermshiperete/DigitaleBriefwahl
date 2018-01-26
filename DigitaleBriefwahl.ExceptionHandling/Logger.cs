// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public static class Logger
	{
		private static string LogDirectory
		{
			get
			{
				if (Platform.IsMac)
				{
					var home = Environment.GetEnvironmentVariable("HOME");
					if (!string.IsNullOrEmpty(home))
						return Path.Combine(home, "Library/Logs/DigitaleBriefwahl");
				}

				return Path.GetTempPath();
			}
		}

		static Logger()
		{
			LogFile = Path.Combine(LogDirectory, $"{AppName}.log");
			Directory.CreateDirectory(LogDirectory);

			Log("-----------------------------");
			Log(DateTime.Now.ToString("u"));
		}

		private static string AppName => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);

		public static string LogFile { get; }

		public static void Log(string text)
		{
			File.AppendAllText(LogFile,
				$"[{Process.GetCurrentProcess().Id}] {text.TrimEnd('\r', '\n')}{Environment.NewLine}");
		}
	}
}