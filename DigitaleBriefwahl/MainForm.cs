// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using Eto.Forms;
using Eto.Drawing;
using DigitaleBriefwahl.Model;
using System.Collections.Generic;
using DigitaleBriefwahl.Views;

namespace DigitaleBriefwahl
{
	/// <summary>
	/// Your application's main form
	/// </summary>
	public class MainForm : Form
	{
		public MainForm()
		{
			var configuration = Configuration.Configure("wycliff.ini");
			Title = configuration.Title;

			ClientSize = new Size(400, 350);
			// scrollable region as the main content
			Content = new Scrollable
			{
				Content = CreateTabControl(configuration)
			};

			// create a few commands that can be used for the menu and toolbar
			var sendCommand = new Command { MenuText = "Absenden", ToolBarText = "Absenden" };
			sendCommand.Executed += OnSendClicked;

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "About my app...");

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem { Text = "&File", Items = { sendCommand } },
					// new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					// new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar
			ToolBar = new ToolBar { Items = { sendCommand } };
		}

		private void OnSendClicked(object sender, EventArgs e)
		{
			var tabControl = ((Scrollable)Content).Content as TabControl;
			var error = false;
			foreach (var page in tabControl.Pages)
			{
				var view = page.Tag as ElectionViewBase;
				if (!view.VerifyOk())
				{
					if (!error)
						tabControl.SelectedPage = page;
					error = true;
				}
			}

			if (error)
				return;

			foreach (var page in tabControl.Pages)
			{
				var view = page.Tag as ElectionViewBase;
				Console.WriteLine(view.GetResult());
			}
			Application.Instance.Quit();
		}

		private TabControl CreateTabControl(Configuration configuration)
		{
			var tabControl = new TabControl();
			foreach (var election in configuration.Elections)
			{
				var view = ElectionViewFactory.Create(election);
				var page = view.Layout();
				page.Tag = view;
				tabControl.Pages.Add(page);
			}
			return tabControl;
		}
	}
}
