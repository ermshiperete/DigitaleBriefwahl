// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using DigitaleBriefwahl.Mail;
using DigitaleBriefwahl.Utils;
using DigitaleBriefwahlTests.Utils;
using NUnit.Framework;

namespace DigitaleBriefwahlTests.Mail
{
	[TestFixture]
	public class MailUtilsTests
	{
		private InMemoryRegistry _registry;
		private InMemoryFile     _file;
		private InMemoryEnvironment _environment;

		[SetUp]
		public void SetUp()
		{
			_registry = new InMemoryRegistry();
			RegistryManager.SetRegistryProvider(_registry);
			_file = new InMemoryFile();
			FileManager.SetFileProvider(_file);
			_environment = new InMemoryEnvironment();
			EnvironmentManager.SetEnvironmentProvider(_environment);
			EmailProviderFactory.SetThunderbirdEmailProviderType(typeof(ThunderbirdEmailProviderFacade));
		}

		[TearDown]
		public void TearDown()
		{
			RegistryManager.Reset();
			FileManager.Reset();
			EnvironmentManager.Reset();
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_Thunderbird()
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.True);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_Thunderbird_NoHKLM()
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.True);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_ThunderbirdOutlook()
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				"Mozilla Thunderbird");
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookSetup_V15()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\01\Outlook\Profiles\01", "",
				"");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.True);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookSetup_V15_Wow()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Wow6432Node\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\01\Outlook\Profiles\01", "",
				"");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.True);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookSetup_V14()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.14");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles\1",
				"", "");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.True);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookNotSetup_V14()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.14");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles",
				"", "");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_NoOffice()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookNotSetup_NoSubkeys()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookNotSetup_WrongProfile1()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\0\Outlook\Profiles\01", "",
				"");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookNotSetup_WrongProfile2()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\XX\Outlook\Profiles\01", "",
				"");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookNotSetup_WrongProfile3()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\01\Outlook", "",
				"");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_OutlookNotSetup_WrongProfile4()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\01\Outlook\Profiles", "",
				"");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		public void CanUsePreferredEmailProvider_Other()
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Foo");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", "Outlook.Application.15");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\01\Outlook\Profiles\01", "",
				"");
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.False);
		}

		[Test]
		[Platform(Include = "Linux")]
		public void CanUsePreferredEmailProvider_Linux_Thunderbird()
		{
			_environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/usr/bin/:/snap/bin");
			_file.SetExistingFile("/usr/bin/thunderbird");

			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.True);
		}

		[Test]
		[Platform(Include = "Linux")]
		public void CanUsePreferredEmailProvider_Linux_Other()
		{
			Assert.That(MailUtils.CanUsePreferredEmailProvider, Is.True);
		}

		[Test]
		[Platform(Include = "Win")]
		[TestCase("15", ExpectedResult = true)]
		[TestCase("11", ExpectedResult = false, Description = "False because of missing profiles")]
		[TestCase("10", ExpectedResult = false)]
		public bool IsOutlookInstalled_Windows(string version)
		{
			((InMemoryRegistryKey)_registry.LocalMachine).CreateKey(@"Software\Clients\Mail", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.ClassesRoot).CreateKey(@"Outlook.Application\CurVer",
				"", $"Outlook.Application.{version}");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office", "",
				"Microsoft Outlook");
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Microsoft\Office\01\Outlook\Profiles\01", "",
				"");
			return MailUtils.IsOutlookInstalled;
		}

		[Test]
		[Platform(Include = "Linux")]
		public void IsOutlookInstalled_Linux()
		{
			Assert.That(MailUtils.IsOutlookInstalled, Is.False);
		}

		[Test]
		[Platform(Include = "Win")]
		[TestCase("Mozilla Thunderbird", true, ExpectedResult = true)]
		[TestCase("Mozilla Thunderbird", false, ExpectedResult = true)]
		[TestCase("", true, ExpectedResult = true)]
		[TestCase("Microsoft Outlook", false, ExpectedResult = false)]
		public bool IsWindowsThunderbirdInstalled_Windows(string value, bool fileExists)
		{
			((InMemoryRegistryKey)_registry.CurrentUser).CreateKey(@"Software\Clients\Mail", "",
				value);
			if (fileExists)
			{
				_file.SetExistingFile(MailUtils.WindowsThunderbirdPath);
			}

			return MailUtils.IsWindowsThunderbirdInstalled;
		}

		[Test]
		[Platform(Include = "Linux")]
		public void IsWindowsThunderbirdInstalled_Linux()
		{
			Assert.That(MailUtils.IsWindowsThunderbirdInstalled, Is.False);
		}

	}
}