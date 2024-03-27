namespace DigitaleBriefwahl.Model
{
	public class ElectionResult
	{
		public int Invalid;

		public virtual void CopyFrom(ElectionResult other)
		{
			Invalid += other.Invalid;
		}
	}
}