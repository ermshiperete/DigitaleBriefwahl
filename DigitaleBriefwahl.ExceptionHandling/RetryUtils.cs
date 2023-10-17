// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Threading;

namespace DigitaleBriefwahl.ExceptionHandling
{
	// TODO: Replace with SIL.Core's RetryUtility
	public static class RetryUtils
	{
		private const int NumberOfRetries = 10;
		private const int DelayOnRetry    = 1000;

		public static void Retry(Action action, int retries = NumberOfRetries, int delay = DelayOnRetry)
		{
			Retry<object>(() => {
				action();
				return null;
			}, retries, delay);
		}

		public static T Retry<T>(Func<T> action, int retries = NumberOfRetries, int delay =
				DelayOnRetry)
		{
			for (var i = 1; i <= retries; i++)
			{
				try
				{
					return action();
				}
				catch (Exception)
				{
					if (i < retries)
						Thread.Sleep(delay);
					else
						throw;
				}
			}

			return default;
		}
	}
}