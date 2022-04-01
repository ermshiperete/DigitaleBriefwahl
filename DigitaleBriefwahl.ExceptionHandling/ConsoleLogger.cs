// Copyright (c) 2022 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

using System;

namespace DigitaleBriefwahl.ExceptionHandling
{
	public class ConsoleLogger: BasicLogger
	{
		public override void Error(string text)
		{
			Console.WriteLine($"ERROR: {text}");
			base.Error(text);
		}
	}
}