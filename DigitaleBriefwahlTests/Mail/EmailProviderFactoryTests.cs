// Copyright (c) 2025 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;
using DigitaleBriefwahl.Mail;
using DigitaleBriefwahl.Utils;
using DigitaleBriefwahlTests.Utils;
using NUnit.Framework;

namespace DigitaleBriefwahlTests.Mail
{
	[TestFixture]
	public class EmailProviderFactoryTests
	{
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

		[TearDown]
		public void TearDown()
		{
			RegistryManager.Reset();
			FileManager.Reset();
		}

		[Test]
		[Platform(Include = "Win")]
		public void GetPreferredEmailProvider_Win_Mapi()
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			var provider = EmailProviderFactory.GetPreferredEmailProvider();
			Assert.That(provider, Is.TypeOf<MapiEmailProvider>());
		}

		[Test]
		[Platform(Include = "Win")]
		public void GetPreferredEmailProvider_Win_Thunderbird()
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			var provider = EmailProviderFactory.GetPreferredEmailProvider();
			Assert.That(provider, Is.TypeOf<ThunderbirdWindowsEmailProvider>());
		}

		[Test]
		[Platform(Include = "Win")]
		public void GetPreferredEmailProvider_Win_Outlook()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Foo");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\01\Outlook\Profiles\01", "",
				"");
			var provider = EmailProviderFactory.GetPreferredEmailProvider();
			Assert.That(provider, Is.TypeOf<OutlookEmailProvider>());
		}

		[Test]
		[Platform(Include = "Win")]
		public void GetPreferredEmailProvider_Win_OutlookToOld_NoProvider()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Foo");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.9");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles",
				"", "");
			var provider = EmailProviderFactory.GetPreferredEmailProvider(false);
			Assert.That(provider, Is.Null);
		}

		[Test]
		[Platform(Include = "Linux")]
		public void GetPreferredEmailProvider_Linux_ThunderbirdDefaultOnX()
		{
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var helpersRcFile = $"{home}/.config/xfce4/helpers.rc";
			_file.SetExistingFile(helpersRcFile);
			_file.AddFileContent(helpersRcFile, "MailReader=thunderbird");

			Assert.That(EmailProviderFactory.GetPreferredEmailProvider(), Is.TypeOf<ThunderbirdEmailProvider>());
		}

		[Test]
		[Platform(Include = "Linux")]
		public void GetPreferredEmailProvider_Linux_XdgEmailExists()
		{
			_file.SetExistingFile("/usr/bin/xdg-email");
			// TODO
			Assert.That(EmailProviderFactory.GetPreferredEmailProvider(), Is.TypeOf<ThunderbirdEmailProvider>());//Is.TypeOf<LinuxEmailProvider>());
		}

		[Test]
		[Platform(Include = "Linux")]
		public void GetPreferredEmailProvider_Linux_Fallback()
		{
			// TODO
			Assert.That(EmailProviderFactory.GetPreferredEmailProvider(), Is.TypeOf<ThunderbirdEmailProvider>());//Is.TypeOf<SIL.Email.MailToEmailProvider>());
		}

		[Test]
		[Platform(Include = "Win")]
		public void AlternateEmailProvider_Win()
		{
			Assert.That(EmailProviderFactory.AlternateEmailProvider(), Is.TypeOf<SIL.Email.MailToEmailProvider>());
		}

		[Test]
		[Platform(Include = "Linux")]
		public void AlternateEmailProvider_Linux_ThunderbirdNotDefault()
		{
			Assert.That(EmailProviderFactory.AlternateEmailProvider(), Is.Null);
		}

		[Test]
		[Platform(Include = "Linux")]
		public void AlternateEmailProvider_Linux_XdgEmailExists()
		{
			_file.SetExistingFile("/usr/bin/xdg-email");
			Assert.That(EmailProviderFactory.AlternateEmailProvider(), Is.TypeOf<SIL.Email.MailToEmailProvider>());
		}

		[Test]
		[Platform(Include = "Linux")]
		public void AlternateEmailProvider_Linux_ThunderbirdDefaultOnX()
		{
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var helpersRcFile = $"{home}/.config/xfce4/helpers.rc";
			_file.SetExistingFile(helpersRcFile);
			_file.AddFileContent(helpersRcFile, "MailReader=thunderbird");

			Assert.That(EmailProviderFactory.AlternateEmailProvider(), Is.TypeOf<SIL.Email.MailToEmailProvider>());
		}

		[Test]
		[Platform(Include = "Linux")]
		public void AlternateEmailProvider_Linux_ThunderbirdDefaultOnGnome()
		{
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var preferredAppFile = $"{home}/.gconf/desktop/gnome/url-handlers/mailto/%gconf.xml";
			_file.SetExistingFile(preferredAppFile);
			_file.AddFileContent(preferredAppFile, @"<?xml version=""1.0""?>
<gconf>
	<entry name='command' type='string'>
		<stringvalue>thunderbird %u</stringvalue>
	</entry>
</gconf>
");

			Assert.That(EmailProviderFactory.AlternateEmailProvider(), Is.TypeOf<SIL.Email.MailToEmailProvider>());
		}
	}
}