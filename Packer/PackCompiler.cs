// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using System.IO.Compression;
using DigitaleBriefwahl.Model;

namespace Packer
{
	internal class PackCompiler
	{
		private Configuration _configuration;

		public PackCompiler(string sourceDir)
		{
			ExecutableLocation = sourceDir;
		}

		public string PackAllFiles()
		{
			_configuration = Configuration.Configure(Path.Combine(ExecutableLocation, Configuration.ConfigName));
			var targetDir = CopyAllFiles();
			try
			{
				var archiveFilename = GetZipFilename();
				CreateZipFile(targetDir, archiveFilename);
				return archiveFilename;
			}
			finally
			{
				Directory.Delete(targetDir, true);
			}
		}

		private string GetZipFilename()
		{
			return Path.Combine(ExecutableLocation, SanitizedElectionName + ".wahl");
		}

		private string SanitizedElectionName =>_configuration.Title.Replace(".", "").Replace(" ", "_");

		private string CopyAllFiles()
		{
			var targetDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(targetDir);
			var dirInfo = new DirectoryInfo(ExecutableLocation);

			foreach (var fileInfo in dirInfo.EnumerateFiles())
			{
				if (fileInfo.Name.StartsWith("Packer.") ||
					fileInfo.Name.Contains("Tests") || fileInfo.Name.StartsWith("nunit.framework") ||
					fileInfo.Name.StartsWith("Wahl") ||
					fileInfo.Name.StartsWith(SanitizedElectionName) ||
					fileInfo.Name.StartsWith("ManuelleVerschluesselung") ||
					Path.GetExtension(fileInfo.Name) == ".wahl")
				{
					continue;
				}

				if (fileInfo.Name.StartsWith("DigitaleBriefwahl.Desktop.exe"))
				{
					File.Copy(fileInfo.Name, Path.Combine(targetDir, fileInfo.Name.Replace("exe", "dll")));
					continue;
				}

				File.Copy(fileInfo.Name, Path.Combine(targetDir, fileInfo.Name));
			}

			return targetDir;
		}

		private string ExecutableLocation { get; }

		private void CreateZipFile(string directory, string archiveFilename)
		{
			if (File.Exists(archiveFilename))
				File.Delete(archiveFilename);
			ZipFile.CreateFromDirectory(directory, archiveFilename);
		}
	}
}
