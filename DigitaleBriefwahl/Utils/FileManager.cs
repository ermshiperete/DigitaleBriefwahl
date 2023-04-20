// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace DigitaleBriefwahl.Utils
{
	public static class FileManager
	{
		public static IFile File { get; private set; } = new FileImpl();

		public static void SetFileProvider(IFile file)
		{
			File = file;
		}

		public static void Reset()
		{
			File = new FileImpl();
		}
	}
}