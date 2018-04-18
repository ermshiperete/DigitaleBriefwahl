// Copyright (c) 2017-2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Bugsnag;
using Bugsnag.Payload;
using SIL.PlatformUtilities;
using Exception = System.Exception;
using StackTrace = System.Diagnostics.StackTrace;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public class ExceptionLogging : Client
	{
		public static ExceptionLogging Client { get; protected set; }

		protected static bool IsInitialized { get; private set; }

		public static void Initialize(string apiKey, string appName,
			string[] args = null, string launcherVersion = null,
			[CallerFilePath] string filename = null)
		{
			Client = new ExceptionLogging(apiKey, appName, args, launcherVersion, filename);
			CommonInitalize();
		}

		protected static void CommonInitalize()
		{
			//Client.AddAnalytics();
			IsInitialized = true;
		}

		private readonly string _solutionPath;

		protected ExceptionLogging(string apiKey, string appName, string[] args,
			string launcherVersion, string callerFilePath)
			: base(apiKey)
		{
			_solutionPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(callerFilePath), ".."));
			if (!_solutionPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				_solutionPath = _solutionPath + Path.DirectorySeparatorChar;

			Setup(launcherVersion, appName, args);
		}

		private static Dictionary<string, string> FindMetadata(string key,
			ICollection<KeyValuePair<string, object>> metadata)
		{
			foreach (var kv in metadata)
			{
				if (kv.Key == key)
					return kv.Value as Dictionary<string, string>;
			}

			var dict = new Dictionary<string, string>();
			metadata.Add(new KeyValuePair<string, object>(key, dict));
			return dict;
		}

		private static string GetOSInfo()
		{
			switch (Environment.OSVersion.Platform)
			{
				// Platform is Windows 95, Windows 98, Windows 98 Second Edition,
				// or Windows Me.
				case PlatformID.Win32Windows:
					// Platform is Windows 95, Windows 98, Windows 98 Second Edition,
					// or Windows Me.
					switch (Environment.OSVersion.Version.Minor)
					{
						case 0:
							return "Windows 95";
						case 10:
							return "Windows 98";
						case 90:
							return "Windows Me";
						default:
							return "UNKNOWN";
					}
				case PlatformID.Win32NT:
					return GetWin32NTVersion();
				case PlatformID.Unix:
				case PlatformID.MacOSX:
					return UnixOrMacVersion();
				default:
					return "UNKNOWN";
			}
		}

		/// <summary>
		/// Detects the current operating system version if its Win32 NT
		/// </summary>
		/// <returns>The operation system version</returns>
		private static string GetWin32NTVersion()
		{
			switch (Environment.OSVersion.Version.Major)
			{
				case 3:
					return "Windows NT 3.51";
				case 4:
					return "Windows NT 4.0";
				case 5:
					return Environment.OSVersion.Version.Minor == 0 ? "Windows 2000" : "Windows XP";
				case 6:
					switch (Environment.OSVersion.Version.Minor)
					{
						case 0:
							return "Windows Server 2008";
						case 1:
							return IsWindowsServer ? "Windows Server 2008 R2" : "Windows 7";
						case 2:
							return IsWindowsServer ? "Windows Server 2012" : "Windows 8";
						case 3:
							return IsWindowsServer ? "Windows Server 2012 R2" : "Windows 8.1";
						default:
							return "UNKNOWN";
					}
				case 10:
					return "Windows 10";
				default:
					return "UNKNOWN";
			}
		}

		// https://stackoverflow.com/a/3138781/487503
		private static bool IsWindowsServer => IsOS(OS_ANYSERVER);

		private const int OS_ANYSERVER = 29;

		[DllImport("shlwapi.dll", SetLastError=true)]
		private static extern bool IsOS(int os);

		/// <summary>
		/// Determines the OS version if on a UNIX based system
		/// </summary>
		/// <returns></returns>
		private static string UnixOrMacVersion()
		{
			if (RunTerminalCommand("uname") == "Darwin")
			{
				var osName = RunTerminalCommand("sw_vers", "-productName");
				var osVersion = RunTerminalCommand("sw_vers", "-productVersion");
				return osName + " (" + osVersion + ")";
			}

			var distro = RunTerminalCommand("bash", "-c \"[ $(which lsb_release) ] && lsb_release -d -s\"");
			return string.IsNullOrEmpty(distro) ? "UNIX" : distro;
		}

		/// <summary>
		/// Executes a command with arguments, used to send terminal commands in UNIX systems
		/// </summary>
		/// <param name="cmd">The command to send</param>
		/// <param name="args">The arguments to send</param>
		/// <returns>The returned output</returns>
		private static string RunTerminalCommand(string cmd, string args = null)
		{
			var proc = new Process {
				EnableRaisingEvents = false,
				StartInfo = {
					FileName = cmd,
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true
				}
			};
			proc.Start();
			proc.WaitForExit();
			var output = proc.StandardOutput.ReadToEnd();
			return output.Trim();
		}

		private void Setup(string launcherVersion, string appName, string[] args)
		{
			BeforeNotify(OnBeforeNotify);

			var configuration = Configuration as Configuration;

			configuration.ProjectNamespaces = new[] { "SIL", "DigitaleBriefwahl" };
			configuration.ProjectRoots = new[] { _solutionPath };
			configuration.AutoCaptureSessions = true;
			configuration.AutoNotify = true;
			configuration.AppType = appName;

			if (string.IsNullOrEmpty(Configuration.ReleaseStage))
			{
				var isDevelopment = Debugger.IsAttached;
#if DEBUG
				isDevelopment = true;
#endif
				if (isDevelopment)
					configuration.ReleaseStage = "development";
			}

			var metadata = new List<KeyValuePair<string, object>>();
			if (configuration.GlobalMetadata != null)
				metadata.AddRange(configuration.GlobalMetadata);

			var app = FindMetadata("App", metadata);
			app.Add("runtime", Platform.IsMono ? "Mono" : ".NET");
			if (Platform.IsMono)
				app.Add("monoversion", Platform.MonoVersion);
			app.Add("appArchitecture", Environment.Is64BitProcess ? "64 bit" : "32 bit");
			app.Add("clrVersion", Environment.Version.ToString());
			app.Add("launcher", launcherVersion);
			app.Add("args", string.Join(", ", args));
			var entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly != null)
			{
				if (string.IsNullOrEmpty(Configuration.AppVersion))
					configuration.AppVersion = entryAssembly.GetName().Version.ToString();

				if (entryAssembly
					.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true)
					.FirstOrDefault() is AssemblyInformationalVersionAttribute informationalVersion)
				{
					app.Add("infoVersion", informationalVersion.InformationalVersion);
				}
			}

			var device = FindMetadata("Device", metadata);
			device.Add("osVersion", GetOSInfo());
			if (!string.IsNullOrEmpty(Environment.OSVersion.ServicePack))
				device.Add("servicePack", Environment.OSVersion.ServicePack);
			device.Add("osArchitecture", Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit");
			device.Add("processorCount", Environment.ProcessorCount + " core(s)");
			device.Add("machineName", Environment.MachineName);
			device.Add("hostName", Dns.GetHostName());
			device.Add("desktop", Platform.DesktopEnvironment);
			if (!string.IsNullOrEmpty(Platform.DesktopEnvironmentInfoString))
				device.Add("shell", Platform.DesktopEnvironmentInfoString);

			var user = FindMetadata("User", metadata);
			user.Add("id", UserId);

			configuration.GlobalMetadata = metadata.ToArray();
		}

		private static string UserId
		{
			get
			{
				// NOTE: we use the hashcode to anonymize the real username
				var securityIdentifier = Platform.IsWindows ? WindowsIdentity.GetCurrent().User : null;
				var hashcode = (uint)(Platform.IsWindows && securityIdentifier != null ?
					securityIdentifier.Value.GetHashCode() :
					Environment.MachineName.GetHashCode() ^ Environment.UserName.GetHashCode());
				return hashcode.ToString();
			}
		}

		private string RemoveFileNamePrefix(string fileName)
		{
			var ret = fileName.StartsWith(_solutionPath)
				? fileName.Substring(_solutionPath.Length)
				: fileName;
			return ret.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		protected virtual void OnBeforeNotify(Report report)
		{
			var logMetadata = FindMetadata("Log", report.Event.Metadata);
			logMetadata.Add("data", Logger.GetLogSinceLastStart());

			var exception = report.OriginalException;
			var stackTrace = new StackTrace(exception, true);
			if (stackTrace.FrameCount <= 0)
				return;

			var frame = stackTrace.GetFrame(0);
//			// During development the line number probably changes frequently, but we want
//			// to treat all errors with the same exception in the same method as being the
//			// same, even when the line numbers differ, so we set it to 0. For releases
//			// we can assume the line number to be constant for a released build.
//			var linenumber = Configuration.ReleaseStage == "development" ? 0 : frame.GetFileLineNumber();
			var linenumber = frame.GetFileLineNumber();
			report.Event.GroupingHash =
				$"{report.OriginalException.GetType().Name} {RemoveFileNamePrefix(frame.GetFileName())} " +
				$"{frame.GetMethod().Name} {linenumber}";
		}

		// ReSharper disable once UnusedMember.Local
		private void AddAnalytics()
		{
			Notify(new AnalyticsException(), Severity.Info);
		}

		public string DataThatWillBeSent
		{
			get
			{
				var bldr = new StringBuilder();
				var configuration = Configuration as Configuration;
				foreach (var metadata in configuration?.GlobalMetadata)
				{
					if (!(metadata.Value is Dictionary<string, string> dict))
						continue;

					foreach (var keyValuePair in dict)
					{
						bldr.AppendLine($"{keyValuePair.Key}={keyValuePair.Value}");
					}
				}

				return bldr.ToString();
			}
		}

		public void Notify(Exception exception, string tab, string key, string value)
		{
			Notify(exception, report =>
			{
				if (report.Event.Metadata.ContainsKey(tab))
				{
					if (report.Event.Metadata[tab] is Dictionary<string, string> dict)
					{
						dict.Add(key, value);
						return;
					}
				}

				report.Event.Metadata.Add(tab, new Dictionary<string, object> { { key, value}});
			});
		}
	}
}
