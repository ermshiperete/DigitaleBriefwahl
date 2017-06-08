// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using Eto.Drawing;
using Eto.Forms;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public partial class ErrorReport : Dialog<bool>
	{
		private Label _label1;
		private Label _label2;
		private Label _label3;

		private void InitializeComponent()
		{
			var appName = Application.Instance.Name;
			Title = $"{appName} Error Report";

			var sendButton = new Button { Text = "&Send" };
			sendButton.Click += OnSendButtonClick;
			var abortButton = new Button { Text = "&Don't Send" };
			abortButton.Click += OnAbortButtonClick;
			var moreInfoButton = new Button { Text = "&More information" };
			moreInfoButton.Click += OnMoreInfoButtonClick;

			Control Spacer() => new Panel {Size = new Size(10, 10)};

			_label1 = new Label
			{
				Text = $"{appName} has encountered a problem and needs to close. " +
					"We are sorry for the inconvenience.",
				Wrap = WrapMode.None
			};
			_label2 = new Label { Text = "Please tell the developers about the problem." };
			_label3 = new Label
			{
				Text = string.Format("{0} created an error report that you can send to help us improve {0}. " +
					"We will treat this report as confidential and anonymous.", appName),
				Wrap = WrapMode.None
			};

			var layout = new StackLayout
			{
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				Items =
				{
					Spacer(),
					new TableLayout(
						new TableRow(
							Spacer(),
							new TableCell(_label1, true),
							Spacer()
						),
						Spacer(),
						new TableRow(
							Spacer(),
							new TableCell(_label2, true),
							Spacer()
						),
						Spacer(),
						new TableRow(
							Spacer(),
							new TableCell(_label3, true),
							Spacer())),
					Spacer(),
					new StackLayoutItem(new StackLayout
						{
							HorizontalContentAlignment = HorizontalAlignment.Stretch,
							Orientation = Orientation.Horizontal,
							Spacing = 5,
							Padding = new Padding(8, 4),
							Items =
							{
								moreInfoButton,
								null,
								abortButton,
								sendButton
							}
						}, VerticalAlignment.Center, true),
				}
			};

			Content = layout;
			DefaultButton = sendButton;
			AbortButton = abortButton;
		}

	}
}
