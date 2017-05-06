// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DigitaleBriefwahl.Model;
using DigitaleBriefwahl.Views;
using Eto.Drawing;
using Eto.Forms;
using SIL.Email;

namespace DigitaleBriefwahl
{
	/// <summary>
	/// Your application's main form
	/// </summary>
	public class MainForm : Form
	{
		private Configuration _configuration;

		public MainForm()
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b");
			_configuration = Configuration.Configure("wahl.ini");
			Title = _configuration.Title;

			ClientSize = new Size(400, 350);
			// scrollable region as the main content
			Content = new Scrollable
			{
				Content = CreateContent(_configuration)
			};

			// create a few commands that can be used for the menu and toolbar
			var sendCommand = new Command { MenuText = "Absenden", ToolBarText = "Absenden" };
			sendCommand.Executed += OnSendClicked;

			var writeCommand = new Command { MenuText = "Wahlzettel schreiben" };
			writeCommand.Executed += OnWriteClicked;

			var quitCommand = new Command { MenuText = "Beenden", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "Über..." };
			aboutCommand.Executed += OnAboutClicked;

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem { Text = "&File", Items = { sendCommand, writeCommand } }
					// new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					// new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
				},
				//				ApplicationItems =
				//				{
				//					// application (OS X) or file menu (others)
				//					new ButtonMenuItem { Text = "&Preferences..." }
				//				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
			Menu.ApplicationMenu.Text = "&Datei";
			Menu.HelpMenu.Text = "&Hilfe";


			// create toolbar
			ToolBar = new ToolBar { Items = { sendCommand } };
		}

		private void OnAboutClicked(object sender, EventArgs e)
		{
			var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			var version = versionInfo.FileVersion;
			MessageBox.Show(
				$"{Configuration.Current.Title}\n\nVersion {version}\n\n{versionInfo.LegalCopyright}",
				"Digitale Briefwahl");
		}

		private void OnSendClicked(object sender, EventArgs e)
		{
			var vote = CollectVote();
			if (string.IsNullOrEmpty(vote))
				return;

			var filename = new Encryption.EncryptVote().WriteVote(Title, vote);

			var emailProvider = EmailProviderFactory.PreferredEmailProvider();
			var email = emailProvider.CreateMessage();

			email.To.Add(_configuration.EmailAddress);
			email.Subject = _configuration.Title;
			email.Body = "Anbei mein Wahlzettel.";
			email.AttachmentFilePath.Add(filename);

			if (emailProvider.SendMessage(email))
				Application.Instance.Quit();

			MessageBox.Show($"Kann E-Mail nicht automatisch verschicken. Bitte die Datei '{filename}' als Anhang einer E-Mail an '{_configuration.EmailAddress}' schicken.");
		}

		private void OnWriteClicked(object sender, EventArgs e)
		{
			var vote = CollectVote();
			if (string.IsNullOrEmpty(vote))
				return;

			var fileName = new Encryption.EncryptVote().WriteVoteUnencrypted(Title, vote);
			MessageBox.Show($"Der Wahlzettel wurde in der Datei '{fileName}' gespeichert.");
		}

		private string CollectVote()
		{
			var tabControl = ((Scrollable)Content).Content as TabControl;
			var error = false;
			foreach (var page in tabControl.Pages)
			{
				var view = page.Tag as ElectionViewBase;
				if (view.VerifyOk())
					continue;

				if (!error)
					tabControl.SelectedPage = page;
				error = true;
			}

			if (error)
				return null;

			var bldr = new StringBuilder();
			bldr.AppendLine(Title);
			bldr.Append('=', Title.Length);
			bldr.AppendLine();
			foreach (var page in tabControl.Pages)
			{
				var view = page.Tag as ElectionViewBase;
				bldr.AppendLine(view.GetResult());
			}
			return bldr.ToString();
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

		private Control CreateContent(Configuration configuration)
		{
			var stackLayout = new StackLayout {Orientation = Orientation.Vertical};

			var item = new StackLayoutItem(CreateTabControl(configuration), VerticalAlignment.Top);
			stackLayout.Items.Add(item);
			var buttonBar = new StackLayout() { Orientation = Orientation.Horizontal,
				HorizontalContentAlignment = HorizontalAlignment.Right};
			buttonBar.Items.Add(new StackLayoutItem(new Button() { Text = "Weiter" }, HorizontalAlignment.Right));
			buttonBar.Items.Add(new StackLayoutItem(new Button() { Text = "Zurück", Enabled = false }, HorizontalAlignment.Right, true));
			stackLayout.Items.Add(buttonBar);
			return stackLayout;
		}
	}
}
