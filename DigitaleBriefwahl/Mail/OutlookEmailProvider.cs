// Copyright (c) 2018-2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace DigitaleBriefwahl.Mail
{
	public class OutlookEmailProvider: WindowsEmailProviderBase
	{
		protected override bool IsApplicable => MailUtils.IsOutlookInstalled;

		protected override string EscapeString(string input)
		{
			return Uri.EscapeDataString(input);
		}

		protected override string FormatString => "-c IPM.Note /m \"{0}{1}{2}&subject={3}&body={4}\"{5}";

		protected override string EmailCommand =>
			// we already checked that the mail client is Outlook
			MailtoCommand;

		private static string GetArguments(IEnumerable<string> arguments)
		{
			var argBuilder = new StringBuilder();

			foreach (var argument in arguments)
			{
				if (argBuilder.Length > 0)
					argBuilder.Append(";");
				argBuilder.Append($"{argument}");
			}

			return argBuilder.ToString();
		}

		protected override string GetToRecipients(IEnumerable<string> recipientTo)
		{
			return GetArguments(recipientTo);
		}

		protected override string GetCcRecipients(ICollection<string> recipients)
		{
			return recipients.Count > 0 ? $"&cc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetBccRecipients(ICollection<string> recipients)
		{
			return recipients.Count > 0 ? $"&bcc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetAttachments(ICollection<string> attachments)
		{
			if (attachments.Count <= 0)
				return null;

			var bldr = new StringBuilder();
			foreach (var file in attachments)
			{
				bldr.Append($" /a \"{file}\"");
			}

			return bldr.ToString();
		}
	}
}