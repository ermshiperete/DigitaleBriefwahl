// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IniParser;
using System.IO;
using IniParser.Model;
using System.Collections.Generic;

namespace DigitaleBriefwahl.Model
{
	public class Configuration
	{
		internal const string ElectionsSection = "Wahlen";
		internal const string TitleKey = "Titel";
		internal const string ElectionKey = "Wahl";
		internal const string DescKey = "Text";
		internal const string DescBeforeKey = "ZwischentextVor";
		internal const string TypeKey = "Typ";
		internal const string VotesKey = "Stimmen";
		internal const string NomineeKey = "Kandidat";
		internal const string NomineeLimitKey = "LimitKandidat";

		private Configuration()
		{
		}

		public string Title { get; private set; }

		public List<ElectionModel> Elections { get; private set; }

		public static Configuration Configure(string filename)
		{
			var configuration = new Configuration();
			if (!File.Exists(filename))
			{
				Console.WriteLine("Can't find configuration file {0}", filename);
				return configuration;
			}

			var parser = new FileIniDataParser();
			var data = parser.ReadFile(filename);

			configuration.Title = data[ElectionsSection][TitleKey];

			configuration.Elections = new List<ElectionModel>();
			foreach (var electionKeyData in data[ElectionsSection])
			{
				if (electionKeyData.KeyName.StartsWith(ElectionKey, StringComparison.InvariantCulture))
					configuration.Elections.Add(new ElectionModel(electionKeyData.Value, data));
			}
			return configuration;
		}
	}
}

