using System.Collections.Generic;
using DigitaleBriefwahl.Utils;

namespace DigitaleBriefwahlTests.Utils
{
	public class InMemoryEnvironment: IEnvironment
	{
		private readonly Dictionary<string, string> _environment = new Dictionary<string, string>();

		public string GetEnvironmentVariable(string variable)
		{
			return _environment.TryGetValue(variable, out var environmentVariable) ? environmentVariable : null;
		}

		public void SetEnvironmentVariable(string variable, string value)
		{
			_environment[variable] = value;
		}
	}
}