// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using Microsoft.Win32;

namespace DigitaleBriefwahl.Utils
{
	public sealed class WindowsRegistry: BaseRegistry
	{
		public override IRegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view)
		{
			var key = RegistryKey.OpenBaseKey(hKey, view);
			return new WindowsRegistryKey(key);
		}
	}
}