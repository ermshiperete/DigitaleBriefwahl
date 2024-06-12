using System.Net;

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

		public override bool Equals(object obj)
		{
			return base.Equals(obj) && obj is YesNoCandidateResult other && other.Yes == Yes &&
				other.No == No && other.Abstention == Abstention;
		}

		public override int GetHashCode()
		{
			var result = 23;
			result = 29 * result + base.GetHashCode();
			result = 29 * result + Yes;
			result = 29 * result + No;
			result = 29 * result + Abstention;
			return result;
		}
	}
}