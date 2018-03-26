// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DigitaleBriefwahl.ExceptionHandling;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.Launcher
{
	/// <summary>
	/// The launcher app has to be installed on the user's system. It will take the URL file,
	/// download the voting app distributed as a zip file, extract and launch it. We expect that
	/// the launcher has to be installed once and can then be used for several years while the
	/// voting app can be updated every year.
	/// </summary>
	internal static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "Launcher", args);

			Logger.Log($"{SquirrelInstallerSupport.Executable} {string.Join(" ", args.Select(s => $"\"{s}\""))}");

			Options options;
			using (var writer = new StreamWriter(Logger.LogFile, true))
			{
				options = Options.ParseCommandLineArgs(writer, args);
				if (options == null)
				{
					Console.WriteLine("Kann übergebene Parameter nicht interpretieren.");
					Logger.Log("Couldn't parse passed in arguments");
					return;
				}
			}

			if (!string.IsNullOrEmpty(options.RunApp) && !File.Exists(options.RunApp))
			{
				Console.WriteLine($"Datei '{options.RunApp}' nicht gefunden.");
				Logger.Log($"Can't find file {options.RunApp}");
				return;
			}

			if (!string.IsNullOrEmpty(options.RunDirectory) && !Directory.Exists(options.RunDirectory))
			{
				Console.WriteLine($"Verzeichnis '{options.RunDirectory}' nicht gefunden.");
				Logger.Log($"Can't find directory {options.RunDirectory}");
				return;
			}

			if (!string.IsNullOrEmpty(options.UrlFile) && !File.Exists(options.UrlFile))
			{
				Console.WriteLine($"URL-Datei '{options.UrlFile}' nicht gefunden.");
				Logger.Log($"Can't find URL file {options.UrlFile}");
				return;
			}

			var didUpdate = UpdateAndLaunchApp(options);

			if (Debugger.IsAttached || didUpdate)
				return;

			if (options.IsInstall)
				Console.WriteLine("Installation erfolgreich.");

			Console.WriteLine("Zum Fortsetzen 'Enter'-Taste drücken");
			Console.ReadLine();
		}

		private static bool UpdateAndLaunchApp(Options options)
		{
			using (var launcher = new Launcher(options.RunDirectory))
			{
				Console.WriteLine(options.SkipUpdateCheck || options.IsInstall ? "Lade Anwendung..." : "Überüfe auf Updates...");

				var didUpdate = UpdateApp(options, launcher).GetAwaiter().GetResult();

				if (string.IsNullOrEmpty(options.RunApp) &&
					string.IsNullOrEmpty(options.RunDirectory) || didUpdate)
				{
					return didUpdate;
				}

				if (!string.IsNullOrEmpty(options.PackageDir))
					SquirrelInstallerSupport.ExecuteUpdatedApp(options);
				else
				{
					Console.WriteLine("Starte Anwendung...");
					launcher.LaunchVotingApp();
				}

				return false;
			}
		}

		[DllImport("wininet.dll")]
		private static extern bool InternetGetConnectedState(out int description, int reservedValue);

		private static async Task<bool> UpdateApp(Options options, Launcher launcher)
		{
			Task<bool> updateManagerTask = null;
			bool didUpdate = false;

			Task<string> unzipVotingApp = null;
			Task<string> downloadVotingApp = null;

			if (Platform.IsWindows)
			{
				if (options.SkipUpdateCheck)
					Console.WriteLine("Ignoriere eventuell vorhandene Updates.");
				else
				{
					if (InternetGetConnectedState(out var flags, 0))
					{
						try
						{
							updateManagerTask = SquirrelInstallerSupport.HandleSquirrelInstallEvent(options);
						}
						catch (HttpRequestException e)
						{
							// some network problem - ignore
							Console.WriteLine("Netzwerkproblem - Überprüfung auf Updates wird übersprungen.");
							Logger.Log(
								$"Network problem - skipping updates: {e.Message} ({e.InnerException?.Message})");
						}
						catch (WebException we)
						{
							// some network problem - ignore
							Console.WriteLine("Netzwerkproblem - Überprüfung auf Updates wird übersprungen.");
							Logger.Log($"Network problem - skipping updates: {we.Message}");
						}
					}
					else
					{
						Console.WriteLine("Netzwerk nicht verbunden - Überprüfung auf Updates wird übersprungen.");
						Logger.Log($"Network not connected - skipping updates (0x{flags:X2})");
					}
				}
			}
			else
				Console.WriteLine("Keine Updates vorhanden.");

			if (!string.IsNullOrEmpty(options.UrlFile))
				downloadVotingApp = DownloadVotingAppFromUrlFile(options.UrlFile);

			if (!string.IsNullOrEmpty(options.RunApp))
				unzipVotingApp = launcher.UnzipVotingAppAsync(options.RunApp);

			if (updateManagerTask != null)
				didUpdate = await updateManagerTask;

			if (downloadVotingApp != null)
			{
				options.RunApp = await downloadVotingApp;
				if (!string.IsNullOrEmpty(options.RunApp))
					unzipVotingApp = launcher.UnzipVotingAppAsync(options.RunApp);
			}

			if (unzipVotingApp != null)
				options.RunDirectory = await unzipVotingApp;

			if (didUpdate && !string.IsNullOrEmpty(options.PackageDir))
				SquirrelInstallerSupport.ExecuteUpdatedApp(options);

			return didUpdate;
		}

		private static async Task<string> DownloadVotingAppFromUrlFile(string urlFile)
		{
			var url = File.ReadAllText(urlFile);
			url.Replace("wahlurl://", "https://");
			var uri = new Uri(url);
			var ballotFile = Path.GetFileNameWithoutExtension(urlFile) + ".wahl";
			Console.WriteLine("Die Wahlunterlagen werden heruntergeladen...");
			Logger.Log($"Downloading from {uri}...");
			var targetFile = await DownloadVotingApp(uri, ballotFile);
			Console.WriteLine("Download abgeschlossen.");
			Logger.Log($"Download of {targetFile} finished.");
			return targetFile;
		}

		private static async Task<string> DownloadVotingApp(Uri uri, string ballotFile)
		{
			var targetFile = Path.Combine(Path.GetTempPath(), ballotFile);
			using (var client = new WebClient())
			{
				client.DownloadFile(uri, targetFile);
			}

			return targetFile;
		}
	}
}