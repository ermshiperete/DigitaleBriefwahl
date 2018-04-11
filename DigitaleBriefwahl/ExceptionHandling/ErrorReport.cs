// Copyright (c) 2017-2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
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

		private static Exception GetInnerException(Exception outer)
		{
			if (outer == null)
				return null;
			while (true)
			{
				if (outer.InnerException == null)
					return outer;
				outer = outer.InnerException;
			}
		}

		private void OnMoreInfoButtonClick(object sender, EventArgs e)
		{
			var ex = GetInnerException(_exception);
			MessageBox.Show(
				$"The following details will be sent:\n{ExceptionLogging.Client.DataThatWillBeSent}\n" +
				$"Exception: {ex?.GetType().Name}\n{ex?.Message}\n\n" +
				$"Stacktrace:\n{_exception?.StackTrace}",
				$"{Application.Instance.Name} Error Report Details");
		}
	}
}

