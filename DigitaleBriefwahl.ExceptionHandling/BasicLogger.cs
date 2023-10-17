// Copyright (c) 2022 Eberhard Beilharz
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
	public class BasicLogger
	{
		private const string SeparatorLine = "-----------------------------";

		public string LogDirectory
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

		public BasicLogger()
		{
			LogFile = Path.Combine(LogDirectory, $"{AppName}.log");
			Initialize();
		}

		protected void Initialize()
		{
			Directory.CreateDirectory(LogDirectory);

			Log(SeparatorLine);
			Log(DateTimeProvider.Current.Now.ToString("u"));
		}

		protected string AppName
		{
			get
			{
				var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
				return Path.GetFileNameWithoutExtension(assembly.Location);
			}
		}

		public string LogFile { get; }

		public virtual void Log(string text)
		{
			WriteLog(text);
		}

		private void WriteLog(string text)
		{
			try
			{
				RetryUtils.Retry(() => File.AppendAllText(LogFile,
					$"[{Process.GetCurrentProcess().Id}] {text.TrimEnd('\r', '\n')}{Environment.NewLine}"));
			}
			catch (IOException)
			{
				// simply ignore if we can't write the file - maybe another instance is running that
				// has the logfile already open
			}
		}

		public virtual void Error(string text)
		{
			WriteLog($"ERROR: {text}");
		}

		public void Truncate()
		{
			try
			{
				File.Delete(LogFile);
			}
			catch (IOException)
			{
				// simply ignore - maybe another instance is running that has the logfile already
				// open
			}
			Initialize();
		}

		public string GetLogSinceLastStart()
		{
			var allLog = File.ReadAllText(LogFile);
			return allLog.Substring(allLog.LastIndexOf($"[{Process.GetCurrentProcess().Id}] {SeparatorLine}"));
		}
	}
}