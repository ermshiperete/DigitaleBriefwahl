namespace DigitaleBriefwahl.Model
{
	public class WeightedElectionResult: ElectionResult
	{
		public int Points { get; internal set; }

		public override void CopyFrom(ElectionResult other)
		{
			base.CopyFrom(other);
			if (other is WeightedElectionResult result)
				Points += result.Points;
		}
	}
}