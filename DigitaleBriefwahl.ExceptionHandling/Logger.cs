// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SIL.PlatformUtilities;
using SIL.Providers;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public static class Logger
	{
		private const string SeparatorLine = "-----------------------------";

		private static string LogDirectory
		{
			get
			{
				// Windows/Linux
				if (!Platform.IsMac)
					return Path.GetTempPath();

				// Mac
				var home = Environment.GetEnvironmentVariable("HOME");
				return !string.IsNullOrEmpty(home)
					? Path.Combine(home, "Library/Logs/DigitaleBriefwahl")
					: Path.GetTempPath();
			}
		}

		static Logger()
		{
			LogFile = Path.Combine(LogDirectory, $"{AppName}.log");
			Initialize();
		}

		private static void Initialize()
		{
			Directory.CreateDirectory(LogDirectory);

			Log(SeparatorLine);
			Log(DateTimeProvider.Current.Now.ToString("u"));
		}

		private static string AppName
		{
			get
			{
				var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
				return Path.GetFileNameWithoutExtension(assembly.Location);
			}
		}

		public static string LogFile { get; }

		public static void Log(string text)
		{
			File.AppendAllText(LogFile,
				$"[{Process.GetCurrentProcess().Id}] {text.TrimEnd('\r', '\n')}{Environment.NewLine}");
		}

		public static void Truncate()
		{
			File.Delete(LogFile);
			Initialize();
		}

		public static string GetLogSinceLastStart()
		{
			var allLog = File.ReadAllText(LogFile);
			return allLog.Substring(allLog.LastIndexOf($"[{Process.GetCurrentProcess().Id}] {SeparatorLine}"));
		}
	}
}