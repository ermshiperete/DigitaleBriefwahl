// Copyright (c) 2018-2021 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DigitaleBriefwahl.ExceptionHandling;
using Microsoft.Win32;
using SIL.PlatformUtilities;
using Squirrel;

namespace DigitaleBriefwahl.Launcher
{
	internal static class SquirrelInstallerSupport
	{
		private const string UpdateUrl = "https://github.com/ermshiperete/DigitaleBriefwahl";

		internal static Task<UpdateManager> GetUpdateManager()
		{
			return UpdateManager.GitHubUpdateManager(UpdateUrl);
		}

		internal static async Task<bool> HandleSquirrelInstallEvent(Options options)
		{
			using var mgr = await UpdateManager.GitHubUpdateManager(UpdateUrl, prerelease: true);

			// WARNING, in most of these scenarios, the app exits at the end of HandleEvents;
			// thus, the method call does not return and nothing can be done after it!
			// We replace two of the usual calls in order to prevent the installation of shortcuts
			if (!options.IsSquirrelCommand || options.FirstRun)
			{
				try
				{
					var updateInfo = await mgr.CheckForUpdate();
					Logger.Log($"Squirrel identified current version '{updateInfo.CurrentlyInstalledVersion?.Version}', " +
								$"future version '{updateInfo.FutureReleaseEntry?.Version}'.");
					if (updateInfo.ReleasesToApply?.Count > 0)
					{
						Console.WriteLine($"Update auf Version '{updateInfo.FutureReleaseEntry?.Version}'.");
						Logger.Log($"Found new update. Applying {updateInfo.ReleasesToApply?.Count} releases. " +
									$"bootstrapping: {updateInfo.IsBootstrapping}, package dir: {updateInfo.PackageDirectory}");
						await mgr.UpdateApp((n) => { Console.Write(new string('.', n * 4 / 10));});
						Console.WriteLine();
						if (!updateInfo.IsBootstrapping)
							options.PackageDir = Path.Combine(Path.GetDirectoryName(updateInfo.PackageDirectory),
								$"app-{updateInfo.FutureReleaseEntry?.Version}");
						return true;
					}
					Logger.Log("No updates");
					if (options.IsInstall || options.FirstRun)
						Console.WriteLine("Keine neuere Version vorhanden.");
					else
						Console.WriteLine("Keine Updates vorhanden. Neueste Version wird bereits ausgeführt.");
				}
				catch (Exception e)
				{
					Logger.Log($"Got exception {e.GetType()}: {e.Message}");
					ExceptionLogging.Client.Notify(e);
				}

				return false;
			}

			// args[0] is command, args[1] version number (at least in some cases)
			if (options.IsInstall || options.IsUpdated)
				MakeRegistryEntries(options);
			else if (options.IsUninstall)
				RemoveRegistryEntries();

			var shortcutLocations = ShortcutLocation.AppRoot | ShortcutLocation.Desktop;
			SquirrelAwareApp.HandleEvents(
				onInitialInstall: v =>
				{
					mgr.CreateShortcutsForExecutable(Path.GetFileName(Executable),
						shortcutLocations,
						false); // not just an update, since this is case initial install
				},
				onAppUpdate: v =>
				{
					mgr.CreateShortcutsForExecutable(Path.GetFileName(Executable),
						shortcutLocations,
						true);
				},
				onAppUninstall: v =>
					mgr.RemoveShortcutsForExecutable(Path.GetFileName(Executable), shortcutLocations)
				);

			return false;
		}

		/// <summary>
		/// True if we consider our install to be shared by all users of the computer.
		/// We currently detect this based on being in the Program Files folder.
		/// </summary>
		/// <returns></returns>
		private static bool SharedByAllUsers()
		{
			// Being a 32-bit app, we expect to get installed in Program Files (x86) on a 64-bit system.
			// If we are in fact on a 32-bit system, we will be in plain Program Files... but on such a
			// system that's what this code gets.
			return LocationOfExecutable.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
		}

		private static string LocationOfExecutable
		{
			get
			{
				var uri = new Uri(Executable);
				return Path.GetDirectoryName(uri.LocalPath);
			}
		}

		internal static string Executable => Assembly.GetExecutingAssembly().Location;

		private static RegistryKey HiveToMakeRegistryKeysIn => SharedByAllUsers() ?
			Registry.LocalMachine.CreateSubKey(@"Software\Classes") :
			Registry.CurrentUser.CreateSubKey(@"Software\Classes");

		private static void RemoveRegistryKey(string parentName, string keyName)
		{
			var root = HiveToMakeRegistryKeysIn;
			var key = string.IsNullOrEmpty(parentName) ? root : root.OpenSubKey(parentName);
			key?.DeleteSubKeyTree(keyName, false);
		}

		private static bool IsFirstTimeInstall(Options options)
		{
			return options.IsInstall;
		}

		private static void EnsureRegistryValue(string keyName, string value, string name = "")
		{
			var root = HiveToMakeRegistryKeysIn;

			var key = root.CreateSubKey(keyName); // may also open an existing key with write permission
			try
			{
				if (key?.GetValue(name) is string current && string.Equals(current, value, StringComparison.InvariantCultureIgnoreCase))
					return; // already set as wanted
				key?.SetValue(name, value);
			}
			catch (UnauthorizedAccessException ex)
			{
				ExceptionLogging.Client.Notify(new UnauthorizedAccessException(
					$"Unable to set registry entry {keyName}:{name} to {value}: {ex.Message}", ex));
			}
		}

		/// <summary>
		/// Make the required registry entries.
		/// We do this every time we run, so that if more than one is installed the latest wins.
		/// </summary>
		private static void MakeRegistryEntries(Options options)
		{
			if (Platform.IsLinux)
			{
				// This will be done by the package installer.
				return;
			}

			if (SharedByAllUsers() && !IsFirstTimeInstall(options))
				return;

			var icon = Path.Combine(LocationOfExecutable, "DigitaleBriefwahl.ico");
			var urlIcon = Path.Combine(LocationOfExecutable, "DigitaleBriefwahlUrl.ico");

			EnsureRegistryValue(".Wahl", "DigitaleBriefwahl.WahlFile");
			EnsureRegistryValue(@".Wahl\DefaultIcon", icon);
			EnsureRegistryValue(".WahlFile", "Digitale Briefwahl");
			EnsureRegistryValue(".WahlFile", "application/wahl", "Content Type");
			EnsureRegistryValue(@".WahlFile\DefaultIcon", icon + ", 0");
			EnsureRegistryValue("DigitaleBriefwahl.Wahl", "Digitale Briefwahl", "FriendlyTypeName");
			EnsureRegistryValue("DigitaleBriefwahl.WahlFile", "Digitale Briefwahl");
			EnsureRegistryValue(@"DigitaleBriefwahl.WahlFile\DefaultIcon", icon + ", 0");
			EnsureRegistryValue(@"DigitaleBriefwahl.WahlFile\shell\open", "Open");
			EnsureRegistryValue(@"DigitaleBriefwahl.WahlFile\shell\open\command", $"\"{Executable}\" --run \"%1\"");
			EnsureRegistryValue(@"DigitaleBriefwahl.Launcher\shell\open\command", $"\"{Executable}\" --run \"%1\"");

			EnsureRegistryValue(".wahlurl", "DigitaleBriefwahl.WahlUrlFile");
			EnsureRegistryValue(@".wahlurl\DefaultIcon", urlIcon);
			EnsureRegistryValue(".WahlUrlFile", "Digitale Briefwahl Download URL");
			EnsureRegistryValue(".WahlUrlFile", "application/wahlurl", "Content Type");
			EnsureRegistryValue(@".WahlUrlFile\DefaultIcon", urlIcon + ", 0");
			EnsureRegistryValue("DigitaleBriefwahl.wahlurl", "Digitale Briefwahl Download URL", "FriendlyTypeName");
			EnsureRegistryValue("DigitaleBriefwahl.WahlUrlFile", "Digitale Briefwahl Download URL");
			EnsureRegistryValue(@"DigitaleBriefwahl.WahlUrlFile\DefaultIcon", urlIcon + ", 0");
			EnsureRegistryValue(@"DigitaleBriefwahl.WahlUrlFile\shell\open", "Open");
			EnsureRegistryValue(@"DigitaleBriefwahl.WahlUrlFile\shell\open\command", $"\"{Executable}\" --url \"%1\"");
		}

		private static void RemoveRegistryEntries()
		{
			RemoveRegistryKey(null, ".Wahl");
			RemoveRegistryKey(null, ".WahlFile");
			RemoveRegistryKey(null, "DigitaleBriefwahl.Wahl");
			RemoveRegistryKey(null, "DigitaleBriefwahl.WahlFile");
			RemoveRegistryKey(null, "DigitaleBriefwahl.Launcher");
			RemoveRegistryKey(null, ".wahlurl");
			RemoveRegistryKey(null, ".WahlUrlFile");
			RemoveRegistryKey(null, "DigitaleBriefwahl.wahlurl");
			RemoveRegistryKey(null, "DigitaleBriefwahl.WahlUrlFile");
		}

		internal static void ExecuteUpdatedApp(Options options)
		{
			var startInfoFileName = Path.Combine(options.PackageDir, Path.GetFileName(Executable));
			var args = string.IsNullOrEmpty(options.RunDirectory)
				? $"--run \"{options.RunApp}\""
				: $"--rundir \"{options.RunDirectory}\"";
			var startInfoArguments = $"--no-check {args}";
			Logger.Log($"Starting new app: '{startInfoFileName}' with args '{startInfoArguments}'");
			Launcher.DeleteOutputDir = false;
			using var process = new Process {
				StartInfo = {
					FileName = startInfoFileName,
					Arguments = startInfoArguments,
					WorkingDirectory = Environment.CurrentDirectory,
					UseShellExecute = false
				}
			};
			process.Start();
		}
	}
}
