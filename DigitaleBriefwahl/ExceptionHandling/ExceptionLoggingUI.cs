// Copyright (c) 2017-2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Runtime.CompilerServices;
using Bugsnag.Payload;
using Eto.Forms;
using Exception = System.Exception;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public class ExceptionLoggingUI: ExceptionLogging
	{
		private Control Parent { get; }

		private ExceptionLoggingUI(string apiKey, string appName, string[] args,
			string launcherVersion, object parent, string callerFilePath)
			: base(apiKey, appName, args, launcherVersion, callerFilePath)
		{
			Parent = parent as Control;
			Application.Instance.UnhandledException += OnUnhandledInstanceException;
		}

		private void OnUnhandledInstanceException(object sender, Eto.UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			Notify(ex, HandledState.ForUnhandledException());
			if (e.IsTerminating)
				Application.Instance.Quit();
		}

		protected override void OnBeforeNotify(Report report)
		{
			var ex = report.OriginalException;
			var errorReport = new ErrorReport(ex);
			if (!IsInitialized || errorReport.ShowModal(Parent))
			{
				base.OnBeforeNotify(report);
				return;
			}

			report.Ignore();
		}

		public static void Initialize(string apiKey, string appName,
			string[] args = null, string launcherVersion = null, object parent = null,
			[CallerFilePath] string filename = null)
		{
			Client = new ExceptionLoggingUI(apiKey, appName, args, launcherVersion, parent, filename);
			CommonInitalize();
		}

	}
}