// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace SelfExtractor
{
	internal class Program
	{
		public static void Main(string[] ignored)
		{
			var ass = Assembly.GetExecutingAssembly();
			var res = ass.GetManifestResourceNames();

			var outputDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(outputDir);

			try
			{
				foreach (var name in res)
				{
					var rs = ass.GetManifestResourceStream(name);

					using (var gzip = new GZipStream(rs, CompressionMode.Decompress, true))
					{
						var path = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(name)); // remove ".gz"

						using (var file = File.Create(path))
						{
							for (int b = gzip.ReadByte(); b != -1; b = gzip.ReadByte())
							{
								file.WriteByte((byte)b);
							}
						}
					}
				}
				Console.WriteLine("Files extracted to {0}", outputDir);
				if (res.Length > 0)
				{
					var exe = "DigitaleBriefwahl.Desktop.exe";
					var program = IsUnix ? "mono" : exe;
					var args = IsUnix ? "--debug " + exe : null;
					using (var process = new Process())
					{
						var startInfo = new ProcessStartInfo(program);
						startInfo.Arguments = args;
						startInfo.WorkingDirectory = outputDir;
						process.StartInfo = startInfo;
						process.Start();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Got exception {0}: {1}", ex.GetType().Name, ex.Message);
				//MessageBox.Show(this, ex.Message, ass.GetName().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static bool IsUnix
		{
			get { return Environment.OSVersion.Platform == PlatformID.Unix; }
		}
	}
}