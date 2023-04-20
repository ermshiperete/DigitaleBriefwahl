// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using Microsoft.Win32;

namespace DigitaleBriefwahl.Utils
{
	public interface IRegistryKey: IDisposable
	{
		IRegistryKey OpenSubKey(string name);

		object GetValue(string name);

		int SubKeyCount { get; }

		string[] GetSubKeyNames();
	}
}