// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;

namespace DigitaleBriefwahl.Utils
{
	public class FileImpl: IFile
	{
		public bool Exists(string path)
		{
			return File.Exists(path);
		}
	}
}