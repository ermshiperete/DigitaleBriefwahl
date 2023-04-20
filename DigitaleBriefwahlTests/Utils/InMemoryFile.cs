// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using DigitaleBriefwahl.Utils;

namespace DigitaleBriefwahlTests.Utils
{
	public class InMemoryFile: IFile
	{
		private readonly HashSet<string> _files = new HashSet<string>();

		public void SetExistingFile(string path)
		{
			_files.Add(path);
		}

		public void Reset()
		{
			_files.Clear();
		}

		public bool Exists(string path)
		{
			return _files.Contains(path);
		}
	}
}