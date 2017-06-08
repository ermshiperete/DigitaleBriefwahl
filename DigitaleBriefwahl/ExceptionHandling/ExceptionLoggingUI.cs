// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using Bugsnag;
using Eto.Forms;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public class ExceptionLoggingUI: ExceptionLogging
	{
		private Control Parent { get; }

		internal ExceptionLoggingUI(string apiKey, object parent, string callerFilePath)
			: base(apiKey, callerFilePath)
		{
			Parent = parent as Control;
			Application.Instance.UnhandledException += OnUnhandledInstanceException;
		}

		private void OnUnhandledInstanceException(object sender, Eto.UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			var errorReport = new ErrorReport(ex);
			if (errorReport.ShowModal(Parent))
			{
				if (ex != null)
				{
					Config.Metadata.AddToTab("Exception Details",
						"stacktrace", ex.StackTrace);
					Notify(ex, Severity.Error);
				}
			}
			Application.Instance.Quit();
		}
	}
}