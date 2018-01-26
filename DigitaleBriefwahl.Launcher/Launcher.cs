// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using DigitaleBriefwahl.ExceptionHandling;

namespace DigitaleBriefwahl.Launcher
{
	internal class Launcher: IDisposable
	{
		private AppDomain VotingAppDomain { get; set; }
		private string OutputDir { get; }

		public static bool DeleteOutputDir { get; set; }

		private string Extension { get; }

		public Launcher(string outputDir, string extension = "dll")
		{
			// If outputDir != null we got called with --rundir. We should delete it when we're done.
			// If outputDir == null we will extract and run the app, so we should delete it as well.
			// In the case where we extract the app but get updated the SquirrelInstallerSupport will
			// set DeleteOutputDir to false and launch the app with --rundir.
			DeleteOutputDir = true;
			OutputDir = string.IsNullOrEmpty(outputDir) ?
				Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) :
				outputDir;
			Extension = extension;
		}

		public void Dispose()
		{
			if (VotingAppDomain != null)
				AppDomain.Unload(VotingAppDomain);
			try
			{
				if (DeleteOutputDir && Directory.Exists(OutputDir))
					Directory.Delete(OutputDir, true);
			}
			catch (UnauthorizedAccessException)
			{
				// just ignore - this can happen when we run in the debugger which still
				// has the .pdb files opened
			}
			catch (IOException)
			{
				// just ignore
			}
			GC.SuppressFinalize(this);
		}

		public async Task<string> UnzipVotingAppAsync(string sourceArchiveFileName)
		{
			return await Task.Run(() => UnzipVotingApp(sourceArchiveFileName));
		}

		public string UnzipVotingApp(string sourceArchiveFileName)
		{
			Directory.CreateDirectory(OutputDir);

			ZipFile.ExtractToDirectory(sourceArchiveFileName, OutputDir);

			return OutputDir;
		}

		public void LaunchVotingApp()
		{
			var setup = new AppDomainSetup { ApplicationBase = OutputDir };

			var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			Environment.CurrentDirectory = OutputDir;
			VotingAppDomain = AppDomain.CreateDomain("VotingApp", null, setup);
			VotingAppDomain.ExecuteAssembly($"DigitaleBriefwahl.Desktop.{Extension}",
				new [] { versionInfo.FileVersion });
		}
	}
}