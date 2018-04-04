// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using Bugsnag;
using Bugsnag.Clients;
using SIL.PlatformUtilities;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public class ExceptionLogging : BaseClient
	{
		protected ExceptionLogging(string apiKey, string appName, string[] args,
			string launcherVersion, string callerFilePath)
			: base(apiKey)
		{
			Setup(launcherVersion, appName, args, callerFilePath);
			//AddAnalytics();
		}

		private void Setup(string launcherVersion, string appName, string[] args, string callerFilePath)
		{
			var solutionPath = Path.GetFullPath(Path.Combine(callerFilePath, "../../"));
			Config.FilePrefixes = new[] { solutionPath };
			Config.UserId = UserId;
			Config.BeforeNotify(OnBeforeNotify);
			Config.StoreOfflineErrors = true;

			Config.Metadata.AddToTab("App", "runtime", Platform.IsMono ? "Mono" : ".NET");
			if (Platform.IsMono)
				Config.Metadata.AddToTab("App", "monoversion", Platform.MonoVersion);
			Config.Metadata.AddToTab("App", "launcher", launcherVersion);
			Config.Metadata.AddToTab("App", "app", appName);
			Config.Metadata.AddToTab("App", "args", string.Join(", ", args));
			Config.Metadata.AddToTab("Device", "desktop", Platform.DesktopEnvironment);
			if (!string.IsNullOrEmpty(Platform.DesktopEnvironmentInfoString))
				Config.Metadata.AddToTab("Device", "shell", Platform.DesktopEnvironmentInfoString);
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
		}

		private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			Notify(e.Exception);
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Notify(e.ExceptionObject as Exception);
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
			var result = fileName;
			if (string.IsNullOrEmpty(result))
				return result;
			foreach (string prefix in Config.FilePrefixes)
			{
				result = result.Replace(prefix, string.Empty);
			}
			return result;
		}

		private bool OnBeforeNotify(Event error)
		{
			var stackTrace = new StackTrace(error.Exception, true);
			if (stackTrace.FrameCount <= 0)
				return true;
			var frame = stackTrace.GetFrame(0);
			// During development the line number probably changes frequently, but we want
			// to treat all errors with the same exception in the same method as being the
			// same, even when the line numbers differ, so we set it to 0. For releases
			// we can assume the line number to be constant for a released build.
			var linenumber = Config.ReleaseStage == "development" ? 0 : frame.GetFileLineNumber();
			error.GroupingHash =
				$"{error.Exception.GetType().Name} {RemoveFileNamePrefix(frame.GetFileName())} " +
				$"{frame.GetMethod().Name} {linenumber}";

			return true;
		}

		public static ExceptionLogging Initialize(string apiKey, string appName,
			string[] args = null, string launcherVersion = null,
			[CallerFilePath] string filename = null)
		{
			Client = new ExceptionLogging(apiKey, appName, args, launcherVersion, filename);
			return Client;
		}

		private void AddAnalytics()
		{
			NotifyAsync(new AnalyticsException(), Severity.Info);
		}

		public static ExceptionLogging Client { get; protected set; }
	}
}

