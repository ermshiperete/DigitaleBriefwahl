// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using IniParser.Model;

namespace DigitaleBriefwahl.Model
{
	public class ElectionModelFactory
	{
		public static ElectionModel Create(string name, IniData data)
		{
			var type = data[name][Configuration.TypeKey];
			if (string.IsNullOrEmpty(type))
				throw new InvalidConfigurationException("Missing type key ('Typ=')");
			switch (type)
			{
				case "YesNo":
					return new YesNoElectionModel(name, data);
				case "Weighted":
					return new WeightedElectionModel(name, data);
				default:
					throw new InvalidConfigurationException($"Unknown election type '{type}' in config file.");
			}
		}
	}
}