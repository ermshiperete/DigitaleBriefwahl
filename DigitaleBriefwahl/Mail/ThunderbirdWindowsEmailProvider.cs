// Copyright (c) 2018-2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DigitaleBriefwahl.Mail
{
	public class ThunderbirdWindowsEmailProvider: WindowsEmailProviderBase
	{
		protected override bool IsApplicable => MailUtils.IsWindowsThunderbirdInstalled &&
			MailtoCommand.Contains("thunderbird");

		protected override string EscapeString(string input)
		{
			return input.Replace(@"""", @"\""");
		}

		protected override string FormatString => "-compose \"to='{0}',subject='{3}',body='{4}'{1}{2}{5}\"";

		protected override string EmailCommand
		{
			get
			{
				// we already checked that the mail client is Thunderbird

				var mailtoCommand = MailtoCommand;
				if (string.IsNullOrEmpty(mailtoCommand))
					return File.Exists(MailUtils.WindowsThunderbirdPath)
						? MailUtils.WindowsThunderbirdPath
						: null;

				if (!mailtoCommand.Contains("ThunderbirdPortable"))
					return mailtoCommand;

				// mailtoCommand might look like "E:\ThunderbirdPortable\App\Thunderbird64\thunderbird.exe" -osint -compose "%1"
				// mailto contains 32-bit and 64-bit versions under ThunderbirdPortable\App.
				// However, the value we got from mailto might not have a profile configured.
				// There's also ThunderbirdPortable\ThunderbirdPortable.exe which is
				// what's usually started, so we use that.
				var regex = new Regex("^(\")?(?<path>.+)thunderbird.exe(\")?", RegexOptions.IgnoreCase);
				var match = regex.Match(mailtoCommand);
				if (!match.Success)
					return mailtoCommand;

				var path = match.Groups["path"].Value;
				var pathRegex = new Regex("(?<start>.+)ThunderbirdPortable[^\"]+");
				var pathMatch = pathRegex.Match(path);
				if (!pathMatch.Success)
					return mailtoCommand;

				var thunderbird = Path.Combine(pathMatch.Groups["start"].Value,
					"ThunderbirdPortable", "ThunderbirdPortable.exe");
				return $"\"{thunderbird}\"";
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