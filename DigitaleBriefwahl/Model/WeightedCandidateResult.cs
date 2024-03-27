namespace DigitaleBriefwahl.Model
{
	public class WeightedCandidateResult: CandidateResult
	{
		public int Points { get; internal set; }

		public override void CopyFrom(CandidateResult other)
		{
			if (other is WeightedCandidateResult result)
				Points += result.Points;
		}
	}
}