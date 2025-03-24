// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.IO;
using DigitaleBriefwahl.Utils;

namespace DigitaleBriefwahlTests.Utils
{
	public class InMemoryFile: IFile
	{
		private readonly HashSet<string> _files = new HashSet<string>();
		private readonly Dictionary<string, string> _fileContents = new Dictionary<string, string>();

		public void SetExistingFile(string path)
		{
			_files.Add(path);
		}

		public void AddFileContent(string path, string content)
		{
			_fileContents.Add(path, content);
		}

		public void Reset()
		{
			_files.Clear();
		}

		public bool Exists(string path)
		{
			return _files.Contains(path);
		}

		public StreamReader OpenText(string path)
		{
			if (!_fileContents.ContainsKey(path))
				throw new FileNotFoundException(path);

			var memoryStream = new MemoryStream();
			var streamWriter = new StreamWriter(memoryStream);
			streamWriter.Write(_fileContents[path]);
			streamWriter.Flush();
			memoryStream.Position = 0;
			return new StreamReader(memoryStream);
		}
	}
}