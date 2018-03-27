// // Copyright (c) 2018 SIL International
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using SIL.Email;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.Mail
{
	public class ThunderbirdWindowsEmailProvider: IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		private static string EscapeString(string input)
		{
			return input.Replace(@"""", @"\""");
		}

		public bool SendMessage(IEmailMessage message)
		{
			if (!Platform.IsWindows || !MailUtils.IsWindowsThunderbirdInstalled)
				return false;

			var body = EscapeString(message.Body);
			var subject = EscapeString(message.Subject);
			var toBuilder = GetToRecipients(message.To);
			var commandLine = $"-osint -compose \"to='{toBuilder}',subject='{subject}',body='{body}'" +
				$"{GetCcRecipients(message.Cc)}{GetBccRecipients(message.Bcc)}{GetAttachments(message.AttachmentFilePath)}\"";
			return StartEmailProcess(commandLine);
		}

		private static string EmailCommand
		{
			get
			{
				// we already checked that the mail client is Thunderbird

				using (var key = Registry.ClassesRoot.OpenSubKey(@"mailto\shell\open\command"))
				{
					var value = key?.GetValue("") as string;
					var regex = new Regex(@"^""([^""]+)""");
					if (value != null && regex.IsMatch(value))
						return regex.Match(value).Groups[1].Value;

					return File.Exists(MailUtils.WindowsThunderbirdPath) ? MailUtils.WindowsThunderbirdPath : null;
				}
			}
		}

		private static bool StartEmailProcess(string commandLine)
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

		private static string GetArguments(IEnumerable<string> arguments, string prefix = "")
		{
			var toBuilder = new StringBuilder();

			foreach (var argument in arguments)
			{
				if (toBuilder.Length > 0)
					toBuilder.Append(",");
				toBuilder.Append($"{prefix}{argument}");
			}

			return toBuilder.ToString();
		}

		private static string GetToRecipients(IEnumerable<string> recipientTo)
		{
			return GetArguments(recipientTo);
		}

		private static string GetCcRecipients(ICollection<string> recipients)
		{
			return recipients.Count > 0 ? $",cc='{GetArguments(recipients)}'" : null;
		}

		private static string GetBccRecipients(ICollection<string> recipients)
		{
			return recipients.Count > 0 ? $",bcc='{GetArguments(recipients)}'" : null;
		}

		private static string GetAttachments(ICollection<string> attachments)
		{
			return attachments.Count > 0 ? $",attachment='{GetArguments(attachments, "file://")}'" : null;
		}
	}
}