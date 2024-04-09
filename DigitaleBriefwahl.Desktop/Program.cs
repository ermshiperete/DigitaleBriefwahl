// Copyright (c) 2016-2023 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.IO;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Model;
using Eto;
using Eto.Forms;

namespace DigitaleBriefwahl.Desktop
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var launcherVersion = args.Length > 0 ? args[0] : null;
			Logger.Log($"DigitaleBriefwahl.Desktop Main starting (version {GitVersionInformation.FullSemVer})");

			var application = new Application(Platform.Detect);
			if (!File.Exists(Configuration.ConfigName))
			{
				MessageBox.Show($"Fehlende {Configuration.ConfigName}");
				Application.Instance.Quit();
				return;
			}

			application.Run(new MainForm(args, launcherVersion));
		}
	}
}
