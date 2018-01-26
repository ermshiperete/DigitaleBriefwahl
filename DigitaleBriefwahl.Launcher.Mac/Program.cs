// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b", "MacLauncher");

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
			using (var launcher = new Launcher(null))
			{
				Logger.Log($"Launching {RunApp}");
				var dir = launcher.UnzipVotingApp(RunApp);
				Logger.Log($"Unzipped app to {dir}");
				launcher.LaunchVotingApp();
				Logger.Log("Returned from launching; quitting app.");
				Application.Instance.Quit();
			}
		}
	}
}