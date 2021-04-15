// Copyright (c) 2021 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

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

			return mapi.Send(message.Subject, message.Body, true);
		}
	}
}