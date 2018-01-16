// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using IniParser.Model;
using System.Collections.Generic;
using System.Text;

namespace DigitaleBriefwahl.Model
{
	public abstract class ElectionModel
	{
		protected ElectionModel(string name, IniData data)
		{
			Name = name;

			Description = data[name][Configuration.DescKey];
			int votes;
			if (!int.TryParse(data[name][Configuration.VotesKey], out votes))
				throw new InvalidConfigurationException("Missing votes key ('Stimmen=')");
			Votes = votes;

			Type = data[name][Configuration.TypeKey] == "YesNo" ? ElectionType.YesNo : ElectionType.Weighted;

			Nominees = new List<string>();
			var nomineeLimitData = new List<KeyData>();
			TextBefore = new List<string>();
			for (var i = 0; i < votes; i++)
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
						throw new InvalidConfigurationException($"Invalid TextBefore key:: {keydata.KeyName}");
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
						$"Invalid nominee limit key: {keydata.KeyName}");
				}
				var limitSplit = keydata.Value.Split('-');
				if (limitSplit.Length < 1 || limitSplit.Length > 2 || keydata.Value.Contains(","))
				{
					throw new InvalidConfigurationException($"Invalid nominee limit value: {keydata.Value}");
				}
				int min;
				int max = 0;
				if (!int.TryParse(limitSplit[0], out min) || min < 1 || min > Votes ||
					(limitSplit.Length > 1 &&
						(!int.TryParse(limitSplit[1], out max) || max < 1 || max > votes)))
				{
					throw new InvalidConfigurationException(
						$"Invalid nominee limit value: {keydata.Value}");
				}
				if (limitSplit.Length <= 1)
					max = min;
				NomineeLimits[Nominees[no - 1]] = new Tuple<int, int>(Math.Min(min, max), Math.Max(min, max));
			}
		}

		public string Name { get; }

		public string Description { get; }

		public List<string> TextBefore { get; }

		public int Votes { get; }

		public ElectionType Type { get; }

		public List<string> Nominees { get; }

		public Dictionary<string, Tuple<int, int>> NomineeLimits { get; }

		public abstract string GetResult(List<string> nominees, bool writeEmptyBallot);

		public abstract List<string> EmptyVotes { get; }

		public override string ToString()
		{
			return
				$"[Election: Name={Name}, Description={Description}, Votes={Votes}, Type={Type}, Nominees={Nominees}]";
		}

		protected static string NormalizeLineEndings(StringBuilder bldr)
		{
			// Normalize line endings so that the ballot has the same length
			// regardless of whether it's run on Windows or Linux. We use Windows line endings.
			return bldr.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n").ToString();
		}
	}
}

