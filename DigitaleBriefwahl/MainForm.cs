// Copyright (c) 2016-2020 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DigitaleBriefwahl.Encryption;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Mail;
using DigitaleBriefwahl.Views;
using Eto.Drawing;
using Eto.Forms;
using SIL.IO;
using Configuration = DigitaleBriefwahl.Model.Configuration;
using Thread = System.Threading.Thread;
using IEmailProvider = SIL.Email.IEmailProvider;

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
		private readonly string _launcherVersion;

		public MainForm(string[] args, string launcherVersion = null)
		{
			ExceptionLoggingUI.Initialize("5012aef9a281f091c1fceea40c03003b", "DigitaleBriefwahl",
				args, launcherVersion, this);

			Logger.Log("MainForm starting");
			Application.Instance.Name = "Digitale Briefwahl";
			_launcherVersion = launcherVersion;

			try
			{
				_configuration = Configuration.Configure(Configuration.ConfigName);
				Title = string.IsNullOrEmpty(_configuration.Title) ? "<Missing>" : _configuration.Title;

				ClientSize = new Size(700, 400);
				// scrollable region as the main content
				Content = new Scrollable
				{
					Content = CreateContent(_configuration)
				};

				// create a few commands that can be used for the menu and toolbar
				var sendCommand = new Command {MenuText = "Absenden", ToolBarText = "Absenden"};
				sendCommand.Executed += OnSendClicked;

				var writeCommand = new Command {MenuText = "Stimmzettel speichern"};
				writeCommand.Executed += OnWriteClicked;

				var writeEncryptedCommand = new Command {MenuText = "Verschlüsselten Stimmzettel speichern"};
				writeEncryptedCommand.Executed += OnWriteEncryptedClicked;

				var writeEmptyCommand = new Command {MenuText = "Leeren Stimmzettel speichern"};
				writeEmptyCommand.Executed += OnWriteEmptyClicked;

				var writeKeyCommand = new Command {MenuText = "Öffentlichen Schlüssel speichern"};
				writeKeyCommand.Executed += OnWritePublicKeyClicked;

				var quitCommand = new Command
				{
					MenuText = "Beenden",
					Shortcut = Application.Instance.CommonModifier | Keys.Q
				};
				quitCommand.Executed += (sender, e) => Application.Instance.Quit();

				var aboutCommand = new Command {MenuText = "Über..."};
				aboutCommand.Executed += OnAboutClicked;

				var sendLogCommand = new Command { MenuText = "Logdateien senden" };
				sendLogCommand.Executed += OnSendLogClicked;

				var viewLogCommand = new Command { MenuText = "Logdateien öffnen" };
				viewLogCommand.Executed += OnViewLogClicked;

				// create menu
				Menu = new MenuBar
				{
					Items =
					{
						// File submenu
						new ButtonMenuItem {Text = "&File",
							Items = {sendCommand, writeCommand, writeEncryptedCommand, writeEmptyCommand, writeKeyCommand}}
					},
					QuitItem = quitCommand,
					AboutItem = aboutCommand
				};
				Menu.ApplicationMenu.Text = "&Datei";
				Menu.HelpMenu.Items.Add(sendLogCommand);
				Menu.HelpMenu.Items.Add(viewLogCommand);
				Menu.HelpMenu.Text = "&Hilfe";
			}
			catch (InvalidConfigurationException ex)
			{
				MessageBox.Show($"Konfigurationsfehler: {ex.Message}", "Digitale Briefwahl");
				Application.Instance.Quit();
			}
		}

		private void OnViewLogClicked(object sender, EventArgs e)
		{
			foreach (var logfile in GetLogfiles())
			{
				PathUtilities.OpenFileInApplication(logfile);
			}
		}

		private void OnSendLogClicked(object sender, EventArgs e)
		{
			IEmailProvider emailProvider;
			var appendLogfileContents = false;
			if (MailUtils.CanUsePreferredEmailProvider)
			{
				emailProvider = SIL.PlatformUtilities.Platform.IsWindows
					? (IEmailProvider)new MapiEmailProvider()
					: (IEmailProvider)new ThunderbirdEmailProvider();
			}
			else if (MailUtils.IsWindowsThunderbirdInstalled)
				emailProvider = new ThunderbirdWindowsEmailProvider();
			else if (MailUtils.IsOutlookInstalled)
				emailProvider = new OutlookEmailProvider();
			else
			{
				emailProvider = EmailProviderFactory.AlternateEmailProvider();
				appendLogfileContents = true;
			}

			try
			{
				Logger.Log($"Sending email with logs through {emailProvider.GetType().Name}");
				var email = emailProvider.CreateMessage();

				email.Subject = "DigiWahl Logfiles";
				email.Body = $"Anbei die Logfiles. Gesendet mittels {emailProvider.GetType().Name}.";
				var logfiles = GetLogfiles();
				if (logfiles?.Length > 0)
				{
					if (appendLogfileContents)
					{
						var bldr = new StringBuilder();
						foreach (var logfile in logfiles)
						{
							bldr.AppendLine();
							bldr.AppendLine($"{Path.GetFileName(logfile)}:");
							bldr.AppendLine(File.ReadAllText(logfile));
						}

						email.Body += bldr.ToString();
					}
					else
					{
						foreach (var logfile in logfiles)
							email.AttachmentFilePath.Add(logfile);
					}
				}
				else
				{
					email.Body += "\nKeine Logfiles gefunden.";
				}

				emailProvider.SendMessage(email);
			}
			catch (Exception ex)
			{
				Logger.Log(
					$"Got {ex.GetType()} exception trying to send email through {emailProvider.GetType().Name}: {ex.Message}");
			}
		}

		private void OnAboutClicked(object sender, EventArgs e)
		{
			var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			var version = versionInfo.FileVersion;
			MessageBox.Show(
				$"Digitale Briefwahl\n\nwahl.ini: {Configuration.Current.Title}\n\n" +
				$"Version {version}\n(Launcher {_launcherVersion})\n\n{versionInfo.LegalCopyright}",
				"Digitale Briefwahl");
		}

		private void OnSendClicked(object sender, EventArgs e)
		{
			var vote = CollectVote();
			if (string.IsNullOrEmpty(vote))
				return;

			var filename = new EncryptVote(Title).WriteVote(vote);

			var mailSent = false;
			string newFileName = null;
			try
			{
				if (MailUtils.CanUsePreferredEmailProvider)
				{
					var emailProvider = SIL.PlatformUtilities.Platform.IsWindows ?
						new MapiEmailProvider() :
						EmailProviderFactory.PreferredEmailProvider();
					mailSent = SendEmail(emailProvider, filename);
					Logger.Log($"Sending email through preferred email provider successful: {mailSent}");
				}

				if (!mailSent && MailUtils.IsWindowsThunderbirdInstalled)
				{
					mailSent = SendEmail(new ThunderbirdWindowsEmailProvider(), filename);
					Logger.Log($"Sending email through Thunderbird on Windows successful: {mailSent}");
				}

				if (!mailSent && MailUtils.IsOutlookInstalled)
				{
					mailSent = SendEmail(new OutlookEmailProvider(), filename);
					Logger.Log($"Sending email through Outlook successful: {mailSent}");
				}

				if (!mailSent)
				{
					newFileName = SaveBallot(
						"Bitte Verzeichnis wählen, in dem der Stimmzettel gespeichert wird!",
						filename);
					var bldr = new StringBuilder();
					bldr.AppendLine(
						"ACHTUNG: Bei der E-Mail, die sich nun öffnet, kann der Stimmzettel unter Umständen " +
						"nicht automatisch angehängt werden.");
					bldr.AppendLine(
						$"Falls das der Fall ist, bitte die Datei '{newFileName}' an die E-Mail anhängen.");

					MessageBox.Show(bldr.ToString(), "Stimmzettel kann u.U. angehängt werden");

					mailSent = SendEmail(EmailProviderFactory.AlternateEmailProvider(), filename);
					Logger.Log($"Sending email through alternate email provider successful: {mailSent}");
				}
			}
			catch (Exception ex)
			{
				ExceptionLogging.Client.Notify(ex, "App", "CaughtException", "Trying to send email");
			}

			if (mailSent)
			{
				Thread.Sleep(100);
				Application.Instance.Quit();
			}
			else
			{
				if (newFileName == null)
				{
					newFileName = SaveBallot(
						"Kann E-Mail nicht automatisch verschicken. Bitte Verzeichnis wählen, in dem der Stimmzettel gespeichert wird!",
						filename);
				}

				if (string.IsNullOrEmpty(newFileName))
					return;

				MessageBox.Show("Konnte E-Mail nicht automatisch verschicken. Bitte die Datei " +
					$"'{newFileName}' als Anhang einer E-Mail an '{_configuration.EmailAddress}' senden.",
					"Automatischer E-Mail-Versand nicht möglich");
			}
		}

		private static string[] GetLogfiles()
		{
			var logFiles = new List<string>();
			var logDesktop = Path.Combine(Logger.LogDirectory, "DigitaleBriefwahl.Desktop.log");
			if (File.Exists(logDesktop))
				logFiles.Add(logDesktop);
			var logLauncher = Path.Combine(Logger.LogDirectory, "DigitaleBriefwahl.Launcher.log");
			if (File.Exists(logLauncher))
				logFiles.Add(logLauncher);
			return logFiles.ToArray();
		}

		private string SaveBallot(string title, string filename)
		{
			using var dialog = new SelectFolderDialog() {
				Title = title,
				Directory = Path.GetDirectoryName(filename)
			};
			var result = dialog.ShowDialog(this);
			if (result != DialogResult.Ok)
				return null;

			var newFilename =
				Path.Combine(dialog.Directory, Path.GetFileName(filename));
			if (filename == newFilename)
				return newFilename;

			File.Copy(filename, newFilename, true);
			File.Delete(filename);

			return newFilename;
		}

		private bool SendEmail(IEmailProvider emailProvider, string filename)
		{
			if (emailProvider == null)
				return false;

			Logger.Log($"Sending email through {emailProvider.GetType().Name}");
			try
			{
				var email = emailProvider.CreateMessage();

				email.To.Add(_configuration.EmailAddress);
				email.Subject = _configuration.Title;
				email.Body = "Anbei mein Stimmzettel.";
				email.AttachmentFilePath.Add(filename);

				return emailProvider.SendMessage(email);
			}
			catch (Exception e)
			{
				Logger.Log($"Got {e.GetType()} exception trying to send email through {emailProvider.GetType().Name}: {e.Message}");
				return false;
			}
		}

		private void WriteBallot(bool writeEmptyBallot, bool writeEncryptedBallot = false)
		{
			var vote = CollectVote(writeEmptyBallot);
			if (string.IsNullOrEmpty(vote))
				return;

			var encryptVote = new EncryptVote(Title);
			using var dialog = new SelectFolderDialog() {
				Title = "Bitte Verzeichnis wählen, in dem der Stimmzettel gespeichert wird",
				Directory = Path.GetDirectoryName(encryptVote.BallotFilePath)
			};
			var result = dialog.ShowDialog(this);
			if (result != DialogResult.Ok)
				return;

			var originalFilePath = Path.Combine(dialog.Directory,
				Path.GetFileName(encryptVote.BallotFilePath));
			var fileName = writeEncryptedBallot ?
				encryptVote.WriteVote(vote, originalFilePath) :
				encryptVote.WriteVoteUnencrypted(vote, originalFilePath);
			var ballotString = writeEmptyBallot ? "leere " : writeEncryptedBallot ? "verschlüsselte " : "";
			MessageBox.Show($"Der {ballotString}Stimmzettel wurde in der Datei '{fileName}' gespeichert.",
				"Stimmzettel gespeichert");
		}

		private void OnWriteClicked(object sender, EventArgs e)
		{
			WriteBallot(false);
		}

		private void OnWriteEncryptedClicked(object sender, EventArgs e)
		{
			WriteBallot(false, true);
		}

		private void OnWriteEmptyClicked(object sender, EventArgs e)
		{
			WriteBallot(true);
		}

		private void OnWritePublicKeyClicked(object sender, EventArgs e)
		{
			var encryptVote = new EncryptVote(Title);
			using var dialog = new SelectFolderDialog() {
				Title = "Bitte Verzeichnis wählen, in dem der öffentliche Schlüssel gespeichert wird",
				Directory = Path.GetDirectoryName(encryptVote.PublicKeyFilePath)
			};
			var result = dialog.ShowDialog(this);
			if (result != DialogResult.Ok)
				return;

			var fileName = encryptVote.WritePublicKey(Path.Combine(dialog.Directory,
				Path.GetFileName(encryptVote.PublicKeyFilePath)));
			MessageBox.Show($"Der öffentliche Schlüssel wurde in der Datei '{fileName}' gespeichert.",
				"Öffentlicher Schlüssel gespeichert");
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

			var results = new List<string>();
			foreach (var page in tabControl.Pages)
			{
				var view = page.Tag as ElectionViewBase;
				results.Add(view.GetResult(writeEmptyBallot));
			}

			return BallotHelper.GetBallot(Title, results);
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
