namespace DigitaleBriefwahl.Model
{
	public class YesNoCandidateResult: CandidateResult
	{
		public int Yes { get; internal set; }
		public int No { get; internal set; }
		public int Abstention { get; internal set; }

		public override void CopyFrom(CandidateResult other)
		{
			if (!(other is YesNoCandidateResult result))
				return;

			Yes += result.Yes;
			No += result.No;
			Abstention += result.Abstention;
		}
	}
}