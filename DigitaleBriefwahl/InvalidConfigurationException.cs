﻿// Copyright (c) 2016 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)
using System;

namespace DigitaleBriefwahl
{
	public class InvalidConfigurationException : ApplicationException
	{
		public InvalidConfigurationException()
		{
		}

		public InvalidConfigurationException(string message) : base(message)
		{
		}
	}
}

