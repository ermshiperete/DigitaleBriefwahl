// Copyright (c) 2017 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using DigitaleBriefwahl;

namespace Packer
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			ExceptionLogging.Initialize("5012aef9a281f091c1fceea40c03003b");
			using (var packer = new PackCompiler())
			{
				packer.PackAllFiles();
			}
		}
	}
}