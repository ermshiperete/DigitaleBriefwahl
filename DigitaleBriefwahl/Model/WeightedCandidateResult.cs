namespace DigitaleBriefwahl.Model
{
	public class WeightedCandidateResult: CandidateResult
	{
		/// <summary>
		/// Default c'tor
		/// </summary>
		public WeightedCandidateResult()
		{
		}

		/// <summary>
		/// C'tor for unit tests
		/// </summary>
		/// <param name="invalid"></param>
		/// <param name="points"></param>
		public WeightedCandidateResult(int invalid, int points)
		{
			Invalid = invalid;
			Points = points;
		}

		public int Points { get; internal set; }

		public override void CopyFrom(CandidateResult other)
		{
			if (other is WeightedCandidateResult result)
				Points += result.Points;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj) && obj is WeightedCandidateResult other && other.Points == Points;
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 19 * result + base.GetHashCode();
			result = 19 * result + Points;
			return result;
		}
	}
}