// Copyright (c) 2025 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using DigitaleBriefwahl.Mail;
using DigitaleBriefwahl.Utils;
using DigitaleBriefwahlTests.Utils;
using NUnit.Framework;

namespace DigitaleBriefwahlTests.Mail
{
	[TestFixture]
	[Platform(Include = "Linux")]
	public class ThunderbirdEmailProviderTests
	{
		private InMemoryFile _file;
		private InMemoryEnvironment _environment;

		[SetUp]
		public void SetUp()
		{
			_file = new InMemoryFile();
			FileManager.SetFileProvider(_file);
			_environment = new InMemoryEnvironment();
			EnvironmentManager.SetEnvironmentProvider(_environment);
		}

		[TearDown]
		public void TearDown()
		{
			FileManager.Reset();
			EnvironmentManager.Reset();
		}

		[Test]
		public void EmailCommand_DebPackage()
		{
			_environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/usr/bin/:/snap/bin");
			_file.SetExistingFile("/usr/bin/thunderbird");
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetEmailCommand(), Is.EqualTo("thunderbird"));
		}

		[Test]
		public void EmailCommand_UsrLocal()
		{
			_environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/usr/bin/:/snap/bin");
			_file.SetExistingFile("/usr/local/bin/thunderbird");
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetEmailCommand(), Is.EqualTo("thunderbird"));
		}

		[Test]
		public void EmailCommand_Snap()
		{
			_environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/usr/bin/:/snap/bin");
			_file.SetExistingFile("/snap/bin/thunderbird");
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetEmailCommand(), Is.EqualTo("thunderbird"));
		}

		[Test]
		public void EmailCommand_Flatpak()
		{
			var emailProvider = new ThunderbirdEmailProviderFacade();
			emailProvider.SetFlatpak(true);

			Assert.That(emailProvider.GetEmailCommand(), Is.EqualTo("flatpak run org.mozilla.Thunderbird"));
		}

		[Test]
		public void EmailCommand_NotAvailable()
		{
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetEmailCommand(), Is.Null);
		}

		[Test]
		public void IsApplicable_DebPackage()
		{
			_environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/usr/bin/:/snap/bin");
			_file.SetExistingFile("/usr/bin/thunderbird");
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetIsApplicable(), Is.True);
		}

		[Test]
		public void IsApplicable_UsrLocal()
		{
			_environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/usr/bin/:/snap/bin");
			_file.SetExistingFile("/usr/local/bin/thunderbird");
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetIsApplicable(), Is.True);
		}

		[Test]
		public void IsApplicable_Snap()
		{
			_environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/usr/bin/:/snap/bin");
			_file.SetExistingFile("/snap/bin/thunderbird");
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetIsApplicable(), Is.True);
		}

		[Test]
		public void IsApplicable_Flatpak()
		{
			var emailProvider = new ThunderbirdEmailProviderFacade();
			emailProvider.SetFlatpak(true);

			Assert.That(emailProvider.GetIsApplicable(), Is.True);
		}

		[Test]
		public void IsApplicable_NotAvailable()
		{
			var emailProvider = new ThunderbirdEmailProviderFacade();

			Assert.That(emailProvider.GetIsApplicable(), Is.False);
		}

	}
}