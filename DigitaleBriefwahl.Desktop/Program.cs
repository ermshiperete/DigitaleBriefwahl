// Copyright (c) 2016-2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;
using Eto;
using Eto.Forms;

namespace DigitaleBriefwahl.Desktop
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(Platform.Detect).Run(new MainForm());
		}
	}
}
