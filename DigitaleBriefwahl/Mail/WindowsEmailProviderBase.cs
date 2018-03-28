// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Diagnostics;
using SIL.Email;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.Mail
{
	public abstract class WindowsEmailProviderBase : IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		protected abstract string EscapeString(string input);

		protected abstract bool IsApplicable { get; }

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

			return p.Start();
		}

		protected abstract string GetToRecipients(IEnumerable<string> recipientTo);
		protected abstract string GetCcRecipients(ICollection<string> recipients);
		protected abstract string GetBccRecipients(ICollection<string> recipients);
		protected abstract string GetAttachments(ICollection<string> attachments);
	}
}