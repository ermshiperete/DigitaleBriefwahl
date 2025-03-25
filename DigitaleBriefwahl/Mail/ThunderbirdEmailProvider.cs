// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DigitaleBriefwahl.Mail
{
	public class ThunderbirdEmailProvider : LinuxEmailProvider
	{
		private static bool? _isThunderbirdFlatpak;
		private const string FlatpakThunderbird = "org.mozilla.Thunderbird";
		private const string Thunderbird = "thunderbird";
		protected override string FormatString => "-compose \"to='{0}',subject='{1}',body='{2}'{3}{4}{5}\"";

		private static string GetArguments(IList<string> arguments, string prefix = "")
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

		private static bool IsOnPath(string fileName)
		{
			if (File.Exists(fileName))
				return true;

			var values = Environment.GetEnvironmentVariable("PATH");
			return !string.IsNullOrEmpty(values) && values.Split(Path.PathSeparator).Select(value => Path.Combine(value, fileName)).Any(path => File.Exists(path));
		}


		protected override string GetToRecipients(IList<string> recipientTo)
		{
			return GetArguments(recipientTo);
		}

		protected override string GetCcRecipients(IList<string> recipients)
		{
			return recipients.Count > 0 ? $",cc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetBccRecipients(IList<string> recipients)
		{
			return recipients.Count > 0 ? $",bcc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetAttachments(IList<string> attachments)
		{
			return attachments.Count > 0 ? $",attachment='{GetArguments(attachments, "file://")}'" : null;
		}

		protected virtual bool IsThunderbirdFlatpak
		{
			get
			{
				if (_isThunderbirdFlatpak.HasValue)
					return _isThunderbirdFlatpak.Value;

				try
				{
					var process = Process.Start("flatpak", $"info {FlatpakThunderbird}");
					if (process == null)
					{
						_isThunderbirdFlatpak = false;
					}
					else
					{
						process.WaitForExit();
						_isThunderbirdFlatpak = process.ExitCode == 0;
					}
				}
				catch
				{
					_isThunderbirdFlatpak = false;
				}

				return _isThunderbirdFlatpak.Value;
			}
		}

		public override bool IsApplicable => IsOnPath(Thunderbird) || IsThunderbirdFlatpak;

		protected override string EmailCommand
		{
			get
			{
				if (!IsApplicable)
					return null;

				return IsThunderbirdFlatpak ? $"flatpak run {FlatpakThunderbird}" : Thunderbird;
			}
		}


	}
}