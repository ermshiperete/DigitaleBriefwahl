// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using DigitaleBriefwahl.Utils;
using Microsoft.Win32;

namespace DigitaleBriefwahlTests.Utils
{
	public class InMemoryRegistry: BaseRegistry
	{
		public override IRegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view)
		{
			switch (hKey)
			{
				case RegistryHive.ClassesRoot:
				case RegistryHive.CurrentUser:
				case RegistryHive.LocalMachine:
					return new InMemoryRegistryKey();
				default:
					throw new NotImplementedException("Unsupported registry hive");
			}
		}
	}
}