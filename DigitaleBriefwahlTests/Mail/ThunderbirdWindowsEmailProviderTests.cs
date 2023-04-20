// Copyright (c) 2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using DigitaleBriefwahl.Mail;
using DigitaleBriefwahl.Utils;
using DigitaleBriefwahlTests.Utils;
using NUnit.Framework;

namespace DigitaleBriefwahlTests.Mail
{
	[TestFixture]
	[Platform(Include = "Win")]
	public class ThunderbirdWindowsEmailProviderTests
	{
		private class ThunderbirdWindowsEmailProviderFacade : ThunderbirdWindowsEmailProvider
		{
			private readonly string _path;

			public ThunderbirdWindowsEmailProviderFacade(string path = null)
			{
				_path = path;
			}

			protected override string MailtoCommand => string.IsNullOrEmpty(_path) ? base.MailtoCommand : _path;

			public string GetEmailCommand() => EmailCommand;
			public bool GetIsApplicable() => IsApplicable;
		}

		private InMemoryRegistry _registry;
		private InMemoryFile     _file;

		[SetUp]
		public void SetUp()
		{
			_registry = new InMemoryRegistry();
			RegistryManager.SetRegistryProvider(_registry);
			_file = new InMemoryFile();
			FileManager.SetFileProvider(_file);
		}

		[Test]
		public void EmailCommand_ThunderbirdPortable()
		{
			const string thunderbird =
				"\"E:\\ThunderbirdPortable\\App\\Thunderbird64\\thunderbird.exe\"";
			var sut = new ThunderbirdWindowsEmailProviderFacade(thunderbird);

			Assert.That(sut.GetEmailCommand(),
				Is.EqualTo("\"E:\\ThunderbirdPortable\\ThunderbirdPortable.exe\""));
		}

		[Test]
		public void EmailCommand_InstalledThunderbird()
		{
			const string thunderbird =
				"\"C:\\Users\\User\\AppData\\Local\\Mozilla Thunderbird\\thunderbird.exe\"";
			var sut = new ThunderbirdWindowsEmailProviderFacade(thunderbird);

			Assert.That(sut.GetEmailCommand(), Is.EqualTo(thunderbird));
		}

		[Test]
		public void IsApplicable_Installed()
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"mailto\shell\open\command",
				"", "\"C:\\Users\\User\\AppData\\Local\\Mozilla Thunderbird\\thunderbird.exe\"");
			var sut = new ThunderbirdWindowsEmailProviderFacade();
			Assert.That(sut.GetIsApplicable, Is.True);
		}

		[Test]
		public void IsApplicable_DifferentCommand()
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"mailto\shell\open\command",
				"", @"""C:\Program Files\Microsoft Office\Root\Office16\OUTLOOK.EXE""");
			var sut = new ThunderbirdWindowsEmailProviderFacade();
			Assert.That(sut.GetIsApplicable, Is.False);
		}
	}
}