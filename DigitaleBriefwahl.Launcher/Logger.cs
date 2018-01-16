// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DigitaleBriefwahl.Launcher
{
	public static class Logger
	{
		static Logger()
		{
			LogFile = Path.Combine(Path.GetTempPath(),
				Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + ".log");
			Log("-----------------------------");
			Log(DateTime.Now.ToString("u"));
		}

		public static string LogFile { get; }

		public static void Log(string text)
		{
			File.AppendAllText(LogFile,
				$"[{Process.GetCurrentProcess().Id}] {text.TrimEnd('\r', '\n')}{Environment.NewLine}");
		}
	}
}