// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace SIL.Providers
{
	public abstract class BaseProvider<T, TDefault> where TDefault : T, new()
	{
		static BaseProvider()
		{
			Current = new TDefault();
		}

		public static T Current { get; private set; }

		public static void ResetToDefault()
		{
			Current = new TDefault();
		}

		public static void SetProvider(T provider)
		{
			if (ReferenceEquals(provider, null))
				throw new ArgumentNullException(nameof(provider));

			Current = provider;
		}
	}

	/// <summary>
	/// Implements a testable DateTime provider. Use this class instead of directly calling
	/// the static methods on <c>DateTime</c>. In your tests you can replace the default DateTime
	/// provider with one that gives reproducible results, e.g. ReproducibleDateTimeProvider.
	/// </summary>
	public abstract class DateTimeProvider
		: BaseProvider<DateTimeProvider, DateTimeProvider.DefaultDateTimeProvider>
	{
		#region DefaultDateTimeProvider
		public class DefaultDateTimeProvider: DateTimeProvider
		{
			public override DateTime Now => DateTime.Now;

			public override DateTime UtcNow => DateTime.UtcNow;

			public override DateTime Today => DateTime.Today;
		}
		#endregion

		public abstract DateTime Now { get; }

		public abstract DateTime UtcNow { get; }

		public abstract DateTime Today { get; }
	}
}
