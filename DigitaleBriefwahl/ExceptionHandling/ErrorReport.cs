// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Net;
using Eto.Forms;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public partial class ErrorReport
	{
		private readonly Exception _exception;

		public ErrorReport(Exception ex)
		{
			_exception = ex;
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			_label1.Wrap = WrapMode.Word;
			_label2.Wrap = WrapMode.Word;
			_label3.Wrap = WrapMode.Word;
			DefaultButton.Focus();
		}

		private void OnSendButtonClick(object sender, EventArgs e)
		{
			Result = true;
			Close();
		}

		private void OnAbortButtonClick(object sender, EventArgs e)
		{
			Result = false;
			Close();
		}

		private void OnMoreInfoButtonClick(object sender, EventArgs e)
		{
			var runtime = SIL.PlatformUtilities.Platform.IsMono
				? $"Mono\nmonoversion={SIL.PlatformUtilities.Platform.MonoVersion}" : ".NET";
			MessageBox.Show(
				$"The following details will be sent:\nhostname={Dns.GetHostName()}\n" +
				$"desktop={SIL.PlatformUtilities.Platform.DesktopEnvironment}\n" +
				$"shell={SIL.PlatformUtilities.Platform.DesktopEnvironmentInfoString}\n" +
				$"processorCount={Environment.ProcessorCount}\n" +
				$"user={ExceptionLogging.Client.Config.UserId}\n" +
				$"runtime={runtime}\n\nStacktrace:\n{_exception?.StackTrace}",
				$"{Application.Instance.Name} Error Report Details");
		}
	}
}

