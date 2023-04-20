// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using Microsoft.Win32;

namespace DigitaleBriefwahl.Utils
{
	public sealed class WindowsRegistryKey: IRegistryKey
	{
		private readonly RegistryKey _registryKey;

		internal WindowsRegistryKey(RegistryKey registryKey)
		{
			_registryKey = registryKey;
		}

		public void Dispose()
		{
			_registryKey?.Dispose();
		}

		public IRegistryKey OpenSubKey(string name)
		{
			var subKey = _registryKey.OpenSubKey(name);
			return subKey == null ? null : new WindowsRegistryKey(subKey);
		}

		public object GetValue(string name) => _registryKey.GetValue(name);

		public int SubKeyCount => _registryKey.SubKeyCount;
		public string[] GetSubKeyNames() => _registryKey.GetSubKeyNames();
	}
}