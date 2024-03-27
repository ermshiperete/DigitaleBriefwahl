using System.Text;

namespace DigitaleBriefwahl.Model
{
	public abstract class CandidateResult
	{
		public int Invalid { get; protected set; }
		public abstract void CopyFrom(CandidateResult other);
	}
}