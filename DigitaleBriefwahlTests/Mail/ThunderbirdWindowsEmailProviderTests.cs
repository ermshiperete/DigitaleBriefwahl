// Copyright (c) 2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using DigitaleBriefwahl.Mail;
using NUnit.Framework;

namespace DigitaleBriefwahlTests.Mail;

[TestFixture]
[Platform(Include = "Win")]
public class ThunderbirdWindowsEmailProviderTests
{
	private class ThunderbirdWindowsEmailProviderFacade: ThunderbirdWindowsEmailProvider
	{
		public ThunderbirdWindowsEmailProviderFacade(string path)
		{
			MailtoCommand = path;
		}

		protected override string MailtoCommand { get; }

		public string GetEmailCommand()
		{
			return EmailCommand;
		}
	}

	[Test]
	public void EmailCommand_ThunderbirdPortable()
	{
		const string thunderbird = "\"E:\\ThunderbirdPortable\\App\\Thunderbird64\\thunderbird.exe\"";
		var sut = new ThunderbirdWindowsEmailProviderFacade(thunderbird);

		Assert.That(sut.GetEmailCommand(), Is.EqualTo("\"E:\\ThunderbirdPortable\\ThunderbirdPortable.exe\""));
	}

	[Test]
	public void EmailCommand_InstalledThunderbird()
	{
		const string thunderbird = "\"C:\\Users\\User\\AppData\\Local\\Mozilla Thunderbird\\thunderbird.exe\"";
		var sut = new ThunderbirdWindowsEmailProviderFacade(thunderbird);

		Assert.That(sut.GetEmailCommand(), Is.EqualTo(thunderbird));
	}
}