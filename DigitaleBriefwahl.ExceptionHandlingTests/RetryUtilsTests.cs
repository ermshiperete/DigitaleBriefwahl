// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using DigitaleBriefwahl.ExceptionHandling;
using NUnit.Framework;

namespace DigitaleBriefwahl.ExceptionHandlingTests
{
	[TestFixture]
	public class RetryUtilsTests
	{
		[Test]
		public void Retry_WorksOnFirstAttempt()
		{
			int n = 0;
			RetryUtils.Retry(() => { n++; }, 3, 1);
			Assert.That(n, Is.EqualTo(1));
		}

		[Test]
		public void Retry_WorksOnSecondAttempt()
		{
			int n = 0;
			RetryUtils.Retry(() =>
			{
				n++;
				if (n == 1)
					throw new ApplicationException();
			}, 3, 1);
			Assert.That(n, Is.EqualTo(2));
		}

		[Test]
		public void Retry_NeverWorks()
		{
			int n = 0;
			Assert.That(() => RetryUtils.Retry(() =>
			{
				n++;
				throw new ApplicationException();
			}, 3, 1), Throws.Exception.TypeOf<ApplicationException>());
			Assert.That(n, Is.EqualTo(3));
		}

		[Test]
		public void Retry_NoWorkFor0()
		{
			int n = 0;
			RetryUtils.Retry(() => { n++; }, 0, 1);
			Assert.That(n, Is.EqualTo(0));
		}
	}
}