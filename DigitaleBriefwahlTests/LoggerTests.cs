// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using SIL.Providers;
using SIL.TestUtilities.Providers;

namespace DigitaleBriefwahl.ExceptionHandling
{
	[TestFixture(TestOf = typeof(Logger))]
	public class LoggerTests
	{
		private DateTime _expectedDateTime;

		[SetUp]
		public void Setup()
		{
			_expectedDateTime = DateTime.Now;
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(_expectedDateTime));
			Logger.Truncate();
		}

		[TearDown]
		public void TearDown()
		{
			DateTimeProvider.ResetToDefault();
		}

		[Test]
		public void Log()
		{
			// Execute
			Logger.Log("Hello World!");

			// Verify
			var procId = Process.GetCurrentProcess().Id;
			Assert.That(File.ReadAllText(Logger.LogFile), Is.EqualTo($@"[{procId}] -----------------------------
[{procId}] {_expectedDateTime:u}
[{procId}] Hello World!
"));
		}

		[Test]
		public void GetLogSinceLastStart()
		{
			// Setup
			Logger.Log("One");
			Logger.Log("Two");
			// Simulate a new program start
			Logger.Log("-----------------------------");
			Logger.Log(DateTimeProvider.Current.Now.ToString("u"));
			Logger.Log("Three");

			// Execute
			var logText = Logger.GetLogSinceLastStart();

			// Verify
			var procId = Process.GetCurrentProcess().Id;
			Assert.That(logText, Is.EqualTo($@"[{procId}] -----------------------------
[{procId}] {_expectedDateTime:u}
[{procId}] Three
"));
		}
	}
}
