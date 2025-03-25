namespace DigitaleBriefwahl.Utils
{
	internal class PathImpl: IPath
	{
		public string GetFullPath(string path)
		{
			return System.IO.Path.GetFullPath(path);
		}

		public string Combine(string path1, string path2)
		{
			return System.IO.Path.Combine(path1, path2);
		}

		public char PathSeparator => System.IO.Path.PathSeparator;
	}
}