// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using DigitaleBriefwahl.Utils;
using SIL.Email;

namespace DigitaleBriefwahl.Mail
{
	public class LinuxEmailProvider : IEmailProvider
	{
		protected static IFile File => FileManager.File;
		protected static IPath Path => FileManager.Path;
		protected static IEnvironment Environment => EnvironmentManager.Environment;

		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		/// <summary>Prepare strings that will later be surrounded in single quotes to be passed to xdg-email via Process.Start in Mono/Linux.</summary>
		public static string EscapeString(string input)
		{
			// Escape backslashes to prevent user from defeating the single-quote-escape below with \', to prevent
			// problems with a backslash at the end of a string (which would result in a backslash escaping the
			// closing quote when the string is later surrounded in single quotes), and also add enough backslashes
			// so they get through to Thunderbird as entered by the user, and sequences like \t\n\0 don't do anything unexpected.
			// 8 backslashes are needed because Process.Start loses 4 of them before calling xdg-email, and then
			// xdg-email loses 3 of them before calling gvfs-open.
			input = input.Replace(@"\", @"\\\\\\\\"); // !
			// Prevent unescaped single quotes from crashing Process.Start().
			input = input.Replace("'", @"\'");
			return input;
		}

		public bool SendMessage(IEmailMessage message)
		{
			string body = EscapeString(message.Body);
			string subject = EscapeString(message.Subject);
			var toBuilder = GetToRecipients(message.To);
			var commandLine = string.Format(
				FormatString,
				toBuilder, subject, body, GetCcRecipients(message.Cc),
				GetBccRecipients(message.Bcc), GetAttachments(message.AttachmentFilePath)
			);
			Console.WriteLine(commandLine);
			return StartEmailProcess(commandLine);
		}

		protected virtual bool StartEmailProcess(string commandLine)
		{
			using var p = new Process();
			p.StartInfo.FileName = EmailCommand;
			p.StartInfo.Arguments = $"{ExtraEmailArgs}{commandLine}";
			p.StartInfo.UseShellExecute = true;
			p.StartInfo.ErrorDialog = true;

			var retVal = p.Start();
			p.WaitForExit();
			return retVal;
		}

		protected virtual string EmailCommand => "xdg-email";
		protected virtual string ExtraEmailArgs => string.Empty;

		protected virtual string FormatString => "--subject '{1}' --body '{2}'{3}{4}{5}{0}";

		private static string GetArguments(IList<string> arguments, string field)
		{
			var toBuilder = new StringBuilder();

			foreach (var argument in arguments)
			{
				toBuilder.Append($" {field}'{argument}'");
			}

			return toBuilder.ToString();
		}

		protected virtual string GetToRecipients(IList<string> recipientTo)
		{
			return GetArguments(recipientTo, "");
		}

		protected virtual string GetCcRecipients(IList<string> recipients)
		{
			return GetArguments(recipients, "--cc ");
		}

		protected virtual string GetBccRecipients(IList<string> recipients)
		{
			return GetArguments(recipients, "--bcc ");
		}

		protected virtual string GetAttachments(IList<string> attachments)
		{
			return GetArguments(attachments, "--attach ");
		}
		public virtual bool IsApplicable => true;
	}
}
