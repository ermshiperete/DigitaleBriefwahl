// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DigitaleBriefwahl.ExceptionHandling;
using IniParser;

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
		internal const string PublicKeyKey = "PublicKey";
		internal const string Email = "Email";
		internal const string MissingOk = "FehlendOk";
		internal const string Abstention = "<Enthaltung>";


		public const string ConfigName = "wahl.ini";

		private Configuration()
		{
			Current = this;
		}

		public static Configuration Current { get; private set; }

		public string Title { get; private set; }

		public List<ElectionModel> Elections { get; private set; }

		public string PublicKey { get; private set; }

		public string EmailAddress { get; private set; }

		public static Configuration Configure(string filename)
		{
			var configuration = new Configuration();
			if (!File.Exists(filename))
			{
				Console.WriteLine("Can't find configuration file {0}", filename);
				Logger.Log($"Can't find configuration file {filename}");
				return configuration;
			}

			var parser = new FileIniDataParser();
			var data = parser.ReadFile(filename, Encoding.UTF8);

			configuration.PublicKey = data[ElectionsSection][PublicKeyKey];

			configuration.Title = data[ElectionsSection][TitleKey];

			configuration.EmailAddress = data[ElectionsSection][Email];

			configuration.Elections = new List<ElectionModel>();
			foreach (var electionKeyData in data[ElectionsSection])
			{
				if (electionKeyData.KeyName.StartsWith(ElectionKey, StringComparison.InvariantCulture))
					configuration.Elections.Add(ElectionModelFactory.Create(electionKeyData.Value, data));
			}
			return configuration;
		}
	}
}
