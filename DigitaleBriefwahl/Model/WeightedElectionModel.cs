// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System.Collections.Generic;
using System.Text;
using IniParser.Model;

namespace DigitaleBriefwahl.Model
{
	public class WeightedElectionModel: ElectionModel
	{
		public WeightedElectionModel(string name, IniData data) : base(name, data)
		{
		}

		public override string GetResult(List<string> electedNominees)
		{
			var bldr = new StringBuilder();
			bldr.AppendLine(Name);
			bldr.Append('-', Name.Length);
			bldr.AppendLine();

			foreach (var nominee in Nominees)
			{
				if (electedNominees.Contains(nominee))
				{
					for (var i = 0; i < Votes; i++)
					{
						if (electedNominees[i] != nominee)
							continue;

						bldr.AppendLine($"{i + 1}. {nominee}");
						break;
					}
				}
				else
				{
					bldr.AppendLine($"   {nominee}");
				}
			}

			return bldr.ToString();
		}
	}
}