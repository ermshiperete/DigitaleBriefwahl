using SIL.Email;

namespace DigitaleBriefwahl.Mail
{
	public interface IEmailProvider
	{
		IEmailMessage CreateMessage();

		bool SendMessage(IEmailMessage message);

		bool IsApplicable { get; }
	}
}