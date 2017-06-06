// This file is based on the article "Self-Extractor" by Thomas Polaert (https://www.codeproject.com/Articles/31475/Self-Extractor)
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;

namespace Packer
{
	class PackCompiler : IDisposable
	{
		// Source file of standalone exe
		private readonly string _sourceName = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SelfExtractor.cs");

		// Compressed files ready to embed as resource
		private List<string> _filenames = new List<string>();

		public void PackAllFiles()
		{
			AddAllFiles();
			CompileArchive("Wahl.exe", null);
		}

		public void AddAllFiles()
		{
			var dirInfo = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			var filesToPack = new List<string>();

			foreach (var fileInfo in dirInfo.EnumerateFiles())
			{
				if (fileInfo.Name.StartsWith("Packer.") || fileInfo.Name == "SelfExtractor.cs")
					continue;

				if (fileInfo.Name.Contains("Tests") || fileInfo.Name == "nunit.framework.dll")
					continue;

				if (fileInfo.Name.StartsWith("Wahl"))
					continue;

				filesToPack.Add(fileInfo.Name);
			}

			foreach (var file in filesToPack)
				AddFile(file);
		}

		public void AddFile(string filename)
		{
			// Compress input file using System.IO.Compression
			using (Stream file = File.OpenRead(filename))
			{
				byte[] buffer = new byte[file.Length];

				if (file.Length != file.Read(buffer, 0, buffer.Length))
					throw new IOException("Unable to read " + filename);

				using (Stream gzFile = File.Create(filename + ".gz"))
				{
					using (Stream gzip = new GZipStream(gzFile, CompressionMode.Compress))
					{
						gzip.Write(buffer, 0, buffer.Length);
					}
				}
			}
			// Store filename so we can embed it on CompileArchive() call
			_filenames.Add(filename + ".gz");
		}

		public void CompileArchive(string archiveFilename, string iconFilename)
		{
			var csc = new CSharpCodeProvider();
			var cp = new CompilerParameters
			{
				GenerateExecutable = true,
				OutputAssembly = archiveFilename,
				CompilerOptions = "/target:winexe"
			};

			if (!string.IsNullOrEmpty(iconFilename))
			{
				cp.CompilerOptions += " /win32icon:" + iconFilename;
			}
			cp.ReferencedAssemblies.Add("System.dll");

			// Add compressed files as resource
			cp.EmbeddedResources.AddRange(_filenames.ToArray());

			// Compile standalone executable with input files embedded as resource
			var cr = csc.CompileAssemblyFromFile(cp, _sourceName);

			// yell if compilation error
			if (cr.Errors.Count <= 0)
			{
				Console.WriteLine("Successfully built 'Wahl.exe'");
				return;
			}

			Console.WriteLine("Errors building {0}", cr.PathToAssembly);

			foreach (CompilerError ce in cr.Errors)
			{
				Console.WriteLine("{0}", ce);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			foreach (string path in _filenames)
			{
				File.Delete(path);
			}
			_filenames.Clear();

			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
