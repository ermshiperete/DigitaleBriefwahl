// Copyright (c) 2018-2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
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

				var client = GetDefaultValue(Registry.CurrentUser, @"Software\Clients\Mail");
				var retVal = !string.IsNullOrEmpty(client) && client == "Mozilla Thunderbird";

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
				var value = GetDefaultValue(Registry.CurrentUser, @"Software\Clients\Mail");
				Logger.Log($@"HKCU\Software\Clients\Mail: {value}");
				if (!string.IsNullOrEmpty(value))
				{
					if (value == "Mozilla Thunderbird")
					{
						var lmValue = GetDefaultValue(Registry.LocalMachine, @"Software\Clients\Mail");
						// if the values in HKLM and HKCU don't agree then MAPI is probably not going
						// to work - so observed with Portable Thunderbird which still brought up
						// non-configured Outlook
						retVal = lmValue == "Mozilla Thunderbird";
						if (!retVal)
						{
							Logger.Log("Not using preferred email provider since HKCU and HKLM disagree");
						}
					}
					else
						retVal = true;
				}
				else
				{
					value = GetDefaultValue(Registry.LocalMachine, @"Software\Clients\Mail");
					Logger.Log($@"HKLM\Software\Clients\Mail: {value}");
					if (!string.IsNullOrEmpty(value))
						retVal = NumberOfOutlookProfiles > 0;
				}

				Logger.Log($"Can use preferred email provider: {retVal}");
				return retVal;
			}
		}

		private static string GetDefaultValue(RegistryKey regKey, string path)
		{
			using var key = regKey.OpenSubKey(path);
			return key?.GetValue("") as string;
		}

		private static int InstalledOutlookVersion
		{
			get
			{
				const string outlookApplication = "Outlook.Application.";
				var outlookVerString = GetDefaultValue(Registry.ClassesRoot, @"Outlook.Application\CurVer");
				if (string.IsNullOrEmpty(outlookVerString) || outlookVerString.Length < outlookApplication.Length)
				{
					Logger.Log("No Outlook installed");
					return -1;
				}

				if (!int.TryParse(outlookVerString.Substring(outlookApplication.Length), out var version))
				{
					Logger.Log($"Outlook not installed (found {outlookVerString})");
					return -1;
				}

				Logger.Log($"Found Outlook version {version}");
				return version;
			}
		}

		private static int NumberOfOutlookProfiles
		{
			get
			{
				int GetNumberOfOutlookProfilesForKey(string keyName)
				{
					using var key = Registry.CurrentUser.OpenSubKey(keyName);
					if (key == null || key.SubKeyCount <= 0)
						return -1;

					foreach (var subkeyName in key.GetSubKeyNames())
					{
						if (subkeyName.Length < 2 || !int.TryParse(subkeyName.Substring(0, 2), out var version))
							continue;
						var subkey = key.OpenSubKey(subkeyName)?.OpenSubKey(@"Outlook\Profiles");
						if (subkey != null)
							return subkey.SubKeyCount;
					}
					return -1;
				}

				int GetNumberOfOutlookProfiles()
				{
					if (InstalledOutlookVersion > 14)
					{
						var ret = GetNumberOfOutlookProfilesForKey(@"Software\Microsoft\Office");
						return ret > 0
							? ret
							: GetNumberOfOutlookProfilesForKey(@"Software\Wow6432Node\Microsoft\Office");
					}

					// location of profiles in older Outlook versions up to Outlook 2010 (14.0)
					using var key = Registry.CurrentUser.OpenSubKey(
						@"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles");
					return key?.SubKeyCount ?? -1;
				}

				var numberOfOutlookProfiles = GetNumberOfOutlookProfiles();
				Logger.Log($"Found {numberOfOutlookProfiles} Outlook profiles");
				return numberOfOutlookProfiles;
			}
		}

	}
}
