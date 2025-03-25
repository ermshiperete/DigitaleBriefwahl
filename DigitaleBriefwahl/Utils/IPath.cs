namespace DigitaleBriefwahl.Utils
{
	public interface IPath
	{
		string GetFullPath(string path);
		string Combine(string path1, string path2);
		char PathSeparator { get; }
	}
}