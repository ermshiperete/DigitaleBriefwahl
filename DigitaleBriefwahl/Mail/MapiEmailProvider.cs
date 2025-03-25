// Copyright (c) 2021-2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using DigitaleBriefwahl.ExceptionHandling;
using SIL.Email;
using Win32Mapi;

namespace DigitaleBriefwahl.Mail
{
	public class MapiEmailProvider: IEmailProvider
	{
		public IEmailMessage CreateMessage()
		{
			return new EmailMessage();
		}

		public bool SendMessage(IEmailMessage message)
		{
			var mapi = new SimpleMapi();

			foreach (var to in message.To)
				mapi.AddRecipient(to, null, false);

			foreach (var cc in message.Cc)
				mapi.AddRecipient(cc, null, true);

			foreach (var bcc in message.Bcc)
				mapi.AddRecipientBCC(bcc, null);

			foreach (var file in message.AttachmentFilePath)
				mapi.Attach(file);

			var result = mapi.Send(message.Subject, message.Body, true);
			if (result)
				return true;

			if (mapi.ErrorValue == SimpleMapi.RETURN_VALUE.MAPI_E_USER_ABORT)
			{
				Logger.Log($"User aborted SendMessage with MapiEmailProvider ({mapi.Error()})");
				return true;
			}

			Logger.Log($"SendMessage with MapiEmailProvider failed with {mapi.Error()}");
			return false;
		}

		public bool IsApplicable => true;
	}
}