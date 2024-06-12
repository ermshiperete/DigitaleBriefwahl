using System.Text;

namespace DigitaleBriefwahl.Model
{
	public abstract class CandidateResult
	{
		public int Invalid { get; protected set; }
		public abstract void CopyFrom(CandidateResult other);

		public override bool Equals(object obj)
		{
			return obj is CandidateResult other && Invalid == other.Invalid;
		}

		public override int GetHashCode()
		{
			return 37 * Invalid;
		}
	}
}