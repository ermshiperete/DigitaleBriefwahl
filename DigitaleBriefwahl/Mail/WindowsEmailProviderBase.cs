﻿// Copyright (c) 2018-2023 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DigitaleBriefwahl.ExceptionHandling;
using DigitaleBriefwahl.Utils;
using SIL.Email;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.Mail
{
	public abstract class WindowsEmailProviderBase : IEmailProvider
	{
		protected IRegistry Registry => RegistryManager.Registry;

		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		protected abstract string EscapeString(string input);

		public abstract bool IsApplicable { get; }

		protected abstract string EmailCommand { get; }
		protected abstract string FormatString { get; }

		public bool SendMessage(IEmailMessage message)
		{
			if (!Platform.IsWindows || !IsApplicable)
				return false;

			var body = EscapeString(message.Body);
			var subject = EscapeString(message.Subject);
			var toBuilder = GetToRecipients(message.To);
			var commandLine = string.Format(FormatString, toBuilder, GetCcRecipients(message.Cc),
				GetBccRecipients(message.Bcc), subject, body, GetAttachments(message.AttachmentFilePath));
			return StartEmailProcess(commandLine);
		}

		private bool StartEmailProcess(string commandLine)
		{
			var p = new Process
			{
				StartInfo =
				{
					FileName = EmailCommand,
					Arguments = commandLine,
					UseShellExecute = false,
					ErrorDialog = true
				}
			};

			Logger.Log($"Sending email: {EmailCommand} {commandLine}");
			try
			{
				return p.Start();
			}
			catch (Win32Exception e)
			{
				Logger.Error($"There was an error opening the file {p.StartInfo.FileName}: {e.Message}");
				return false;
			}
		}

		protected abstract string GetToRecipients(IEnumerable<string> recipientTo);
		protected abstract string GetCcRecipients(ICollection<string> recipients);
		protected abstract string GetBccRecipients(ICollection<string> recipients);
		protected abstract string GetAttachments(ICollection<string> attachments);

		protected virtual string MailtoCommand
		{
			get
			{
				using var key = Registry.ClassesRoot.OpenSubKey(@"mailto\shell\open\command");
				var value = key?.GetValue("") as string;
				var regex = new Regex(@"^""([^""]+)""");
				if (value == null || !regex.IsMatch(value))
					return null;

				return regex.Match(value).Groups[1].Value;
			}
		}
	}
}