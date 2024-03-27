namespace DigitaleBriefwahl.Model
{
	public class YesNoElectionResult: ElectionResult
	{
		public int Yes { get; internal set; }
		public int No { get; internal set; }
		public int Abstain { get; internal set; }
	}
}