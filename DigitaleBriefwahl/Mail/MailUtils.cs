// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using DigitaleBriefwahl.ExceptionHandling;
using Microsoft.Win32;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.Mail
{
	public static class MailUtils
	{
		public static bool IsWindowsThunderbirdInstalled
		{
			get
			{
				if (!Platform.IsWindows)
					return false;

				bool retVal;
				using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Clients\Mail"))
				{
					var client = key?.GetValue("") as string;
					retVal = !string.IsNullOrEmpty(client) && client == "Mozilla Thunderbird";
				}

				if (!retVal)
				{
					retVal = File.Exists(WindowsThunderbirdPath);
				}

				Logger.Log($"Thunderbird is installed on Windows: {retVal}");
				return retVal;
			}
		}

		public static bool IsOutlookInstalled => Platform.IsWindows &&
			InstalledOutlookVersion > 10 && // minimum Outlook 2003 (11.0)
			NumberOfOutlookProfiles > 0;

		public static string WindowsThunderbirdPath => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
			"Mozilla Thunderbird", "thunderbird.exe");

		public static bool CanUsePreferredEmailProvider
		{
			get
			{
				if (!Platform.IsWindows)
					return true;

				var retVal = false;
				if (!string.IsNullOrEmpty(GetDefaultValue(Registry.CurrentUser, @"Software\Clients\Mail")))
					retVal = true;
				else if (!string.IsNullOrEmpty(GetDefaultValue(Registry.LocalMachine, @"Software\Clients\Mail")))
					retVal = NumberOfOutlookProfiles > 0;

				Logger.Log($"Can use perferred email provider: {retVal}");
				return retVal;
			}
		}

		private static string GetDefaultValue(RegistryKey regKey, string path)
		{
			using (var key = regKey.OpenSubKey(path))
			{
				return key?.GetValue("") as string;
			}
		}

		private static int InstalledOutlookVersion
		{
			get
			{
				const string outlookApplication = "Outlook.Application.";
				var outlookVerString = GetDefaultValue(Registry.ClassesRoot, @"Outlook.Application\CurVer");
				if (string.IsNullOrEmpty(outlookVerString) || outlookVerString.Length < outlookApplication.Length)
					return -1;

				if (!int.TryParse(outlookVerString.Substring(outlookApplication.Length), out var version))
					return -1;

				Logger.Log($"Found Outlook version {version}");
				return version;
			}
		}

		private static int GetNumberOfOutlookProfiles(string keyName)
		{
			using (var key = Registry.CurrentUser.OpenSubKey(keyName))
			{
				if (key == null || key.SubKeyCount <= 0)
					return -1;

				foreach (var subkeyName in key.GetSubKeyNames())
				{
					if (subkeyName.Length < 2 || !int.TryParse(subkeyName.Substring(0, 2), out var version))
						continue;
					var subkey = key.OpenSubKey(subkeyName)?.OpenSubKey(@"Outlook\Profiles");
					return subkey?.SubKeyCount ?? -1;
				}
				return -1;
			}
		}

		private static int NumberOfOutlookProfiles
		{
			get
			{
				if (InstalledOutlookVersion > 14)
				{
					var ret = GetNumberOfOutlookProfiles(@"Software\Microsoft\Office");
					return ret > 0 ? ret : GetNumberOfOutlookProfiles(@"Software\Wow6432Node\Microsoft\Office");
				}

				// location of profiles in older Outlook versions up to Outlook 2010 (14.0)
				using (var key = Registry.CurrentUser.OpenSubKey(
					@"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles"))
				{
					return key?.SubKeyCount ?? -1;
				}
			}
		}

	}
}
