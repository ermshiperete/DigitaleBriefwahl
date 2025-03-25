namespace DigitaleBriefwahl.Utils
{
	internal class EnvironmentImpl: IEnvironment
	{
		public string GetEnvironmentVariable(string variable)
		{
			return System.Environment.GetEnvironmentVariable(variable);
		}

		public void SetEnvironmentVariable(string variable, string value)
		{
			System.Environment.SetEnvironmentVariable(variable, value);
		}
	}
}