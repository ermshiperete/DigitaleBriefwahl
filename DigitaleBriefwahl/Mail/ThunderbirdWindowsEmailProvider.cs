// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace DigitaleBriefwahl.Mail
{
	public class ThunderbirdWindowsEmailProvider: WindowsEmailProviderBase
	{
		protected override bool IsApplicable => MailUtils.IsWindowsThunderbirdInstalled;

		protected override string EscapeString(string input)
		{
			return input.Replace(@"""", @"\""");
		}

		protected override string FormatString => "-osint -compose \"to='{0}',subject='{3}',body='{4}'{1}{2}{5}\"";

		protected override string EmailCommand
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

		protected override string GetToRecipients(IEnumerable<string> recipientTo)
		{
			return GetArguments(recipientTo);
		}

		protected override string GetCcRecipients(ICollection<string> recipients)
		{
			return recipients.Count > 0 ? $",cc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetBccRecipients(ICollection<string> recipients)
		{
			return recipients.Count > 0 ? $",bcc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetAttachments(ICollection<string> attachments)
		{
			return attachments.Count > 0 ? $",attachment='{GetArguments(attachments, "file://")}'" : null;
		}
	}
}