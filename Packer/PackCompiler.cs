// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using DigitaleBriefwahl.Model;

namespace Packer
{
	internal class PackCompiler
	{
		public Configuration Config { get; }

		public PackCompiler(string sourceDir)
		{
			ExecutableLocation = sourceDir;
			Config = Configuration.Configure(ConfigFilename);
		}

		public string PackAllFiles()
		{
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

		public string ConfigFilename => Path.Combine(ExecutableLocation, Configuration.ConfigName);

		private string GetZipFilename()
		{
			return Path.Combine(ExecutableLocation, SanitizedElectionName + ".wahl");
		}

		private string SanitizedElectionName =>Config.Title.Replace(".", "").Replace(" ", "_");

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
					fileInfo.Name.StartsWith("Ude") ||
					fileInfo.Name.StartsWith("git2-") ||
					fileInfo.Name.StartsWith("LibGit2Sharp") ||
					fileInfo.Name.StartsWith("Octokit") ||
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

		public static void ConvertToUtf8(string configFileName)
		{
			var encoding = Encoding.UTF8;
			char[] chars;
			int charsUsed;
			byte[] buffer;
			int bufferLength;
			int bytesUsed;
			bool completed;
			using (var fileStream = File.OpenRead(configFileName))
			{
				var charsetDetector = new Ude.CharsetDetector();
				charsetDetector.Feed(fileStream);
				charsetDetector.DataEnd();
				if (charsetDetector.Charset != null)
				{
					encoding = Encoding.GetEncoding(charsetDetector.Charset);
				}

				if (encoding == Encoding.UTF8)
					return;

				fileStream.Seek(0, SeekOrigin.Begin);
				bufferLength = (int)fileStream.Length;
				buffer = new byte[bufferLength];
				fileStream.Read(buffer, 0, bufferLength);

				chars = new char[bufferLength * 2];
				encoding.GetDecoder().Convert(buffer, 0, bufferLength, chars, 0, bufferLength * 2,
					true, out bytesUsed, out charsUsed, out completed);
			}

			Encoding.UTF8.GetEncoder().Convert(chars, 0, charsUsed, buffer, 0, bufferLength, true,
				out charsUsed, out bytesUsed, out completed);
			var sizedBuffer = new byte[bytesUsed];
			Array.Copy(buffer, sizedBuffer, bytesUsed);
			File.WriteAllBytes(configFileName, sizedBuffer);
		}
	}
}
