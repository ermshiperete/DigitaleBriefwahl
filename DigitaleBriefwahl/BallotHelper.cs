// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitaleBriefwahl
{
	public static class BallotHelper
	{
		private static string _ballotId;

		public static string BallotId
		{
			get
			{
				if (_ballotId != null)
					return _ballotId;

				var random = new Random();
				_ballotId = $"{random.Next():X8}-{random.Next():X8}-{random.Next():X8}-{random.Next():X8}";

				return _ballotId;
			}
		}

		public static string GetBallot(string electionTitle, List<string> electionResults)
		{
			var bldr = new StringBuilder();
			bldr.AppendLine(electionTitle);
			bldr.Append('=', electionTitle.Length);
			bldr.AppendLine();
			bldr.AppendLine();
			foreach (var result in electionResults)
			{
				bldr.AppendLine(result);
			}

			bldr.AppendLine();

			// append random number so that two otherwise identical ballots show up differently
			// when encrypted
			bldr.AppendLine(BallotId);

			return bldr.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n").ToString();
		}
	}
}
