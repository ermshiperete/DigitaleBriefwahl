// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using Microsoft.Win32;

namespace DigitaleBriefwahl.Utils
{
	public abstract class BaseRegistry: IRegistry
	{
		public BaseRegistry()
		{
			ClassesRoot = OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);
			LocalMachine = OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
			CurrentUser = OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
		}

		public abstract IRegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view);

		public IRegistryKey ClassesRoot { get; }
		public IRegistryKey LocalMachine { get; }
		public IRegistryKey CurrentUser { get; }

		public virtual void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				ClassesRoot?.Dispose();
				LocalMachine?.Dispose();
				CurrentUser?.Dispose();
			}
		}
	}
}