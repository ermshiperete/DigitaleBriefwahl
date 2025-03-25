namespace DigitaleBriefwahl.Utils
{
	public static class EnvironmentManager
	{
		public static IEnvironment Environment { get; private set; } = new EnvironmentImpl();

		public static void SetEnvironmentProvider(IEnvironment environment)
		{
			Environment = environment;
		}

		public static void Reset()
		{
			Environment = new EnvironmentImpl();
		}
	}
}