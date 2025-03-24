// Copyright (c) 2008-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using DigitaleBriefwahl.Utils;
using SIL.PlatformUtilities;
using IEmailProvider = SIL.Email.IEmailProvider;

namespace DigitaleBriefwahl.Mail
{
	public static class EmailProviderFactory
	{
		private static IFile File => FileManager.File;

		public static IEmailProvider GetPreferredEmailProvider(bool returnAlternateProvider = true)
		{
			if (MailUtils.CanUsePreferredEmailProvider)
			{
				return Platform.IsWindows
					? (IEmailProvider)new MapiEmailProvider()
					: (IEmailProvider)new ThunderbirdEmailProvider();
			}
			if (MailUtils.IsWindowsThunderbirdInstalled)
				return new ThunderbirdWindowsEmailProvider();
			if (MailUtils.IsOutlookInstalled)
				return new OutlookEmailProvider();
			return returnAlternateProvider ? AlternateEmailProvider() : null;
		}

		public static IEmailProvider AlternateEmailProvider()
		{
			if (Platform.IsLinux && !ThunderbirdIsDefault() && !File.Exists("/usr/bin/xdg-email"))
			{
				return null;
			}
			return new SIL.Email.MailToEmailProvider();
		}

		public static bool ThunderbirdIsDefault()
		{
			var result = ThunderbirdIsDefaultOnX();
			if (!result)
				result |= ThunderbirdIsDefaultOnGnome();
			return result;
		}

		public static bool ThunderbirdIsDefaultOnGnome()
		{
			var result = false;
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var preferredAppFile = $"{home}/.gconf/desktop/gnome/url-handlers/mailto/%gconf.xml";
			if (File.Exists(preferredAppFile))
			{
				var doc = new XPathDocument(new XmlTextReader(preferredAppFile));
				var nav = doc.CreateNavigator();
				var it = nav.Select("/gconf/entry/stringvalue");
				if (it.MoveNext())
				{
					result = it.Current.Value.Contains("thunderbird");
				}
			}
			Console.WriteLine("Thunderbird on Gnome? Result {0}", result);
			return result;
		}

		public static bool ThunderbirdIsDefaultOnX()
		{
			var result = false;
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var preferredAppFile = $"{home}/.config/xfce4/helpers.rc";
			if (File.Exists(preferredAppFile))
			{
				using var reader = File.OpenText(preferredAppFile);
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine()?.Trim();
					var keyValue = line?.Split('=');
					if (keyValue?.Length != 2)
						continue;

					result = keyValue[0] == "MailReader" && keyValue[1] == "thunderbird";
					break;
				}
			}
			Console.WriteLine("Thunderbird on X? Result {0}", result);
			return result;
		}

	}
}
