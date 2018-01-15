// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using DigitaleBriefwahl.ExceptionHandling;

namespace DigitaleBriefwahlLauncher
{
	/// <summary>
	/// The launcher app has to be installed on the user's system. It will take the voting app
	/// distributed as a zip file, extract and launch it. We expect that the launcher has to be
	/// installed once and can then be used for several years while the voting app can be updated
	/// every year.
	/// </summary>
	internal static class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length < 2 || args[0] != "--run")
			{
				Console.WriteLine("Missing/wrong parameters");
				return;
			}

			if (!File.Exists(args[1]))
			{
				Console.WriteLine($"Can't find file {args[1]}");
				return;
			}

			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b");
			var outputDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				Directory.CreateDirectory(outputDir);

				ZipFile.ExtractToDirectory(args[1], outputDir);

				Environment.CurrentDirectory = outputDir;
				var assembly = Assembly.LoadFile(Path.Combine(outputDir, "DigitaleBriefwahl.Desktop.exe"));
				var programType = assembly.GetType("DigitaleBriefwahl.Desktop.Program");
				var mainMethod = programType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public);
				mainMethod.Invoke(null, new[] {new string[0]});
			}
			finally
			{
				Directory.Delete(outputDir, true);
			}
		}
	}
}