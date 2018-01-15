// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DigitaleBriefwahl.ExceptionHandling;
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
		private readonly Configuration _configuration;
		private TabControl _tabControl;
		private Button _nextButton;
		private Button _backButton;

		public MainForm()
		{
			ExceptionLoggingUI.Initialize("5012aef9a281f091c1fceea40c03003b", this);
			Application.Instance.Name = "Digitale Briefwahl";

			try
			{
				_configuration = Configuration.Configure("wahl.ini");
				Title = _configuration.Title;

				ClientSize = new Size(700, 400);
				// scrollable region as the main content
				Content = new Scrollable
				{
					Content = CreateContent(_configuration)
				};

				// create a few commands that can be used for the menu and toolbar
				var sendCommand = new Command {MenuText = "Absenden", ToolBarText = "Absenden"};
				sendCommand.Executed += OnSendClicked;

				var writeCommand = new Command {MenuText = "Stimmzettel schreiben"};
				writeCommand.Executed += OnWriteClicked;

				var writeEmptyCommand = new Command {MenuText = "Leeren Stimmzettel schreiben"};
				writeEmptyCommand.Executed += OnWriteEmptyClicked;

				var writeKeyCommand = new Command {MenuText = "Öffentlichen Schlüssel schreiben"};
				writeKeyCommand.Executed += OnWritePublicKeyClicked;

				var quitCommand = new Command
				{
					MenuText = "Beenden",
					Shortcut = Application.Instance.CommonModifier | Keys.Q
				};
				quitCommand.Executed += (sender, e) => Application.Instance.Quit();

				var aboutCommand = new Command {MenuText = "Über..."};
				aboutCommand.Executed += OnAboutClicked;

				// create menu
				Menu = new MenuBar
				{
					Items =
					{
						// File submenu
						new ButtonMenuItem {Text = "&File",
							Items = {sendCommand, writeCommand, writeEmptyCommand, writeKeyCommand}}
					},
					QuitItem = quitCommand,
					AboutItem = aboutCommand
				};
				Menu.ApplicationMenu.Text = "&Datei";
				Menu.HelpMenu.Text = "&Hilfe";
			}
			catch (InvalidConfigurationException ex)
			{
				MessageBox.Show($"Konfigurationsfehler: {ex.Message}", "Digitale Briefwahl");
				Application.Instance.Quit();
			}
		}

		private void OnAboutClicked(object sender, EventArgs e)
		{
			var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			var version = versionInfo.FileVersion;
			MessageBox.Show(
				$"Digitale Briefwahl\n\nwahl.ini: {Configuration.Current.Title}\n\nVersion {version}\n\n{versionInfo.LegalCopyright}",
				"Digitale Briefwahl");
		}

		private void OnSendClicked(object sender, EventArgs e)
		{
			var vote = CollectVote();
			if (string.IsNullOrEmpty(vote))
				return;

			var filename = new Encryption.EncryptVote().WriteVote(Title, vote);

			if (SendEmail(EmailProviderFactory.PreferredEmailProvider(), filename) ||
				SendEmail(EmailProviderFactory.AlternateEmailProvider(), filename))
			{
				Application.Instance.Quit();
			}
			else
			{
				MessageBox.Show(
					$"Kann E-Mail nicht automatisch verschicken. Bitte die Datei '{filename}' als Anhang einer E-Mail an '{_configuration.EmailAddress}' schicken.");
			}
		}

		private bool SendEmail(IEmailProvider emailProvider, string filename)
		{
			var email = emailProvider.CreateMessage();

			email.To.Add(_configuration.EmailAddress);
			email.Subject = _configuration.Title;
			email.Body = "Anbei mein Stimmzettel.";
			email.AttachmentFilePath.Add(filename);

			return emailProvider.SendMessage(email);
		}

		private void WriteBallot(bool writeEmptyBallot)
		{
			var vote = CollectVote(writeEmptyBallot);
			if (string.IsNullOrEmpty(vote))
				return;

			var fileName = new Encryption.EncryptVote().WriteVoteUnencrypted(Title, vote);
			MessageBox.Show($"Der Stimmzettel wurde in der Datei '{fileName}' gespeichert.");
		}

		private void OnWriteClicked(object sender, EventArgs e)
		{
			WriteBallot(false);
		}

		private void OnWriteEmptyClicked(object sender, EventArgs e)
		{
			WriteBallot(true);
		}

		private void OnWritePublicKeyClicked(object sender, EventArgs e)
		{
			var fileName = Encryption.EncryptVote.WritePublicKey(Title);
			MessageBox.Show($"Der öffentliche Schlüssel wurde in der Datei '{fileName}' gespeichert.");
		}

		private string CollectVote(bool writeEmptyBallot = false)
		{
			var dynamicLayout = ((Scrollable)Content).Content as DynamicLayout;
			var tabControl = dynamicLayout.Children.First() as TabControl;
			var error = false;
			if (!writeEmptyBallot)
			{
				foreach (var page in tabControl.Pages)
				{
					var view = page.Tag as ElectionViewBase;
					if (view.VerifyOk())
						continue;

					if (!error)
						tabControl.SelectedPage = page;
					error = true;
				}
			}

			if (error)
				return null;

			var bldr = new StringBuilder();
			bldr.AppendLine(Title);
			bldr.Append('=', Title.Length);
			bldr.AppendLine();
			bldr.AppendLine();
			foreach (var page in tabControl.Pages)
			{
				var view = page.Tag as ElectionViewBase;
				bldr.AppendLine(view.GetResult(writeEmptyBallot));
			}
			return bldr.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n").ToString();
		}

		private TabControl CreateTabControl(Configuration configuration)
		{
			_tabControl = new TabControl();
			foreach (var election in configuration.Elections)
			{
				var view = ElectionViewFactory.Create(election);
				var page = view.Layout();
				page.Tag = view;
				_tabControl.Pages.Add(page);
			}
			_tabControl.SelectedIndexChanged += OnSelectedIndexChanged;
			return _tabControl;
		}

		private Control CreateContent(Configuration configuration)
		{
			var stackLayout = new DynamicLayout();
			stackLayout.BeginVertical();
			stackLayout.Add(CreateTabControl(configuration));
			stackLayout.EndVertical();

			stackLayout.BeginVertical();
			stackLayout.BeginHorizontal();
			stackLayout.Add(null, true);
			var buttonBar = new StackLayout()
			{
				Orientation = Orientation.Horizontal,
			};
			_backButton = new Button { Text = "Zurück", Enabled = false };
			var backButtonCommand = new Command { MenuText = "Zurück" };
			backButtonCommand.Executed += OnBackClicked;
			_backButton.Command = backButtonCommand;
			buttonBar.Items.Add(new StackLayoutItem(_backButton, HorizontalAlignment.Left, true));
			_nextButton = new Button { Text = "Weiter" };
			var nextButtonCommand = new Command { MenuText = "Weiter" };
			nextButtonCommand.Executed += OnNextClicked;
			_nextButton.Command = nextButtonCommand;
			buttonBar.Items.Add(new StackLayoutItem(_nextButton, HorizontalAlignment.Left));
			stackLayout.Add(buttonBar);
			stackLayout.EndHorizontal();
			stackLayout.EndVertical();
			return stackLayout;
		}

		private void OnNextClicked(object sender, EventArgs e)
		{
			if (_tabControl.SelectedIndex < _tabControl.Pages.Count - 1)
				_tabControl.SelectedIndex++;
			else
			{
				OnSendClicked(sender, e);
				new Command { MenuText = "Weiter" }.Enabled = false;
			}
		}

		private void OnBackClicked(object sender, EventArgs e)
		{
			_tabControl.SelectedIndex--;
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			_nextButton.Enabled = true;
			_nextButton.Text = _tabControl.SelectedIndex < _tabControl.Pages.Count - 1 ? "Weiter" : "Abschicken";
			_backButton.Enabled = _tabControl.SelectedIndex > 0;
		}

	}
}
