// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using Microsoft.Win32;

namespace DigitaleBriefwahl.Utils
{
	public interface IRegistry: IDisposable
	{
		IRegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view);

		IRegistryKey ClassesRoot { get; }
		IRegistryKey LocalMachine { get; }
		IRegistryKey CurrentUser { get; }
	}
}