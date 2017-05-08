// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using IniParser.Model;
using System.Collections.Generic;

namespace DigitaleBriefwahl.Model
{
	public class ElectionModel
	{
		public ElectionModel(string name, IniData data)
		{
			Name = name;

			Description = data[name][Configuration.DescKey];
			int votes;
			if (!int.TryParse(data[name][Configuration.VotesKey], out votes))
				throw new InvalidConfigurationException("Missing votes key ('Stimmen=')");
			Votes = votes;

			Type = data[name][Configuration.TypeKey] == "JN" ? ElectionType.YesNo : ElectionType.Weighted;

			Nominees = new List<string>();
			var nomineeLimitData = new List<KeyData>();
			TextBefore = new List<string>();
			for (int i = 0; i < votes; i++)
				TextBefore.Add(null);

			foreach (var keydata in data[name])
			{
				if (keydata.KeyName.StartsWith(Configuration.NomineeKey, StringComparison.CurrentCulture))
					Nominees.Add(keydata.Value);
				else if (keydata.KeyName.StartsWith(Configuration.NomineeLimitKey, StringComparison.CurrentCulture))
					nomineeLimitData.Add(keydata);
				else if (keydata.KeyName.StartsWith(Configuration.DescBeforeKey, StringComparison.CurrentCulture))
				{
					var noString = keydata.KeyName.Substring(Configuration.DescBeforeKey.Length);
					int no;
					if (!int.TryParse(noString, out no) || no < 1 || no > Votes)
					{
						throw new InvalidConfigurationException(
							string.Format("Invalid TextBefore key:: {0}", keydata.KeyName));
					}
					TextBefore[no - 1] = keydata.Value;
				}
			}

			NomineeLimits = new Dictionary<string, Tuple<int, int>>();
			foreach (var keydata in nomineeLimitData)
			{
				var noString = keydata.KeyName.Substring(Configuration.NomineeLimitKey.Length);
				int no;
				if (!int.TryParse(noString, out no) || no < 1 || no > Nominees.Count)
				{
					throw new InvalidConfigurationException(
						string.Format("Invalid nominee limit key: {0}", keydata.KeyName));
				}
				var limitSplit = keydata.Value.Split('-');
				if (limitSplit.Length < 1 || limitSplit.Length > 2 || keydata.Value.Contains(","))
				{
					throw new InvalidConfigurationException(
						string.Format("Invalid nominee limit value: {0}", keydata.Value));
				}
				int min;
				int max = 0;
				if (!int.TryParse(limitSplit[0], out min) || min < 1 || min > Votes ||
					(limitSplit.Length > 1 &&
						(!int.TryParse(limitSplit[1], out max) || max < 1 || max > votes)))
				{
					throw new InvalidConfigurationException(
						string.Format("Invalid nominee limit value: {0}", keydata.Value));
				}
				if (limitSplit.Length <= 1)
					max = min;
				NomineeLimits[Nominees[no - 1]] = new Tuple<int, int>(Math.Min(min, max), Math.Max(min, max));
			}
		}

		public string Name { get; private set; }

		public string Description { get; private set; }

		public List<string> TextBefore { get; private set; }

		public int Votes { get; private set; }

		public ElectionType Type { get; private set; }

		public List<string> Nominees { get; private set; }

		public Dictionary<string, Tuple<int, int>> NomineeLimits { get; private set; }

		public override string ToString()
		{
			return string.Format("[Election: Name={0}, Description={1}, Votes={2}, Type={3}, Nominees={4}]", Name, Description, Votes, Type, Nominees);
		}
	}
}

