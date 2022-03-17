// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using DigitaleBriefwahl.ExceptionHandling;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using MonoDevelop.MacInterop;

namespace DigitaleBriefwahl.Launcher.Mac
{
	class MyForm : Form
	{
		public MyForm()
		{
			Application.Instance.Name = "Digitale Briefwahl";

			// sets the client (inner) size of the window for your content
			ClientSize = new Size(600, 400);

			Title = "Digitale Briefwahl Launcher for Mac OS X";
			Visible = false;
		}
	}

	internal static class Program
	{
		private static string Executable => Assembly.GetExecutingAssembly().Location;
		private static string RunApp;

		[STAThread]
		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "MacLauncher", args);

			var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			Logger.Log($"{Executable} {string.Join(" ", args.Select(s => $"\"{s}\""))} ({versionInfo.FileVersion})");

			ApplicationEvents.OpenDocuments += (sender, eventArgs) =>
			{
				if (eventArgs.Documents?.Count > 0)
				{
					RunApp = eventArgs.Documents.Keys.First();
					Logger.Log($"Got document {RunApp}");
					Application.Instance.AsyncInvoke(LaunchApp);
				}

				eventArgs.Handled = true;
			};

			new Application(Platform.Detect).Run(new MyForm());
			Logger.Log("End of main"); // will never come here
		}

		private static void LaunchApp()
		{
			try
			{
				if (Path.GetExtension(RunApp) == ".wahlurl")
				{
					RunApp = DownloadVotingAppFromUrlFile(RunApp);
					if (string.IsNullOrEmpty(RunApp))
						return;
				}

				using var launcher = new Launcher(null);
				Logger.Log($"Launching {RunApp}");
				var dir = launcher.UnzipVotingApp(RunApp);
				Logger.Log($"Unzipped app to {dir}");
				launcher.LaunchVotingApp();
				Logger.Log("Returned from launching; quitting app.");
			}
			catch (CannotUnloadAppDomainException e)
			{
				// just ignore
				Logger.Log($"Got {e.GetType()} trying to dispose launcher - IGNORED.");
			}
			finally
			{
				Application.Instance.Quit();
			}
		}

		private static string DownloadVotingAppFromUrlFile(string urlFile)
		{
			try
			{
				Logger.Log($"Reading URL from {urlFile}");
				var url = File.ReadAllText(urlFile);
				url.Replace("wahlurl://", "https://");
				Logger.Log($"Downloading app from {url}");
				var uri = new Uri(url);
				var ballotFile = Path.GetFileNameWithoutExtension(urlFile) + ".wahl";
				return DownloadVotingApp(uri, ballotFile);
			}
			catch (Exception e)
			{
				Logger.Log($"Got {e.GetType()} exception trying to download URL: {e.Message}");
				return null;
			}
		}

		private static string DownloadVotingApp(Uri uri, string ballotFile)
		{
			var targetFile = Path.Combine(Path.GetTempPath(), ballotFile);
			using var client = new WebClient();
			client.DownloadFile(uri, targetFile);

			return targetFile;
		}
	}
}