using System.Collections.Generic;

namespace DigitaleBriefwahl.Model
{
	public class BallotResult
	{
		public BallotResult()
		{
			WeightedRanks = new List<int>();
			YesNoVotes = new List<string>();
		}

		public List<int> WeightedRanks { get; }

		public List<string> YesNoVotes { get; }

		public bool Invalid { get; set; }
		public bool Incomplete { get; set; }
	}
}
