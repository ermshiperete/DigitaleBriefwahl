namespace DigitaleBriefwahl.Model
{
	public class YesNoElectionResult: ElectionResult
	{
		public int Yes { get; internal set; }
		public int No { get; internal set; }
		public int Abstain { get; internal set; }

		public override void CopyFrom(ElectionResult other)
		{
			base.CopyFrom(other);

			if (!(other is YesNoElectionResult result))
				return;

			Yes += result.Yes;
			No += result.No;
			Abstain += result.Abstain;
		}
	}
}