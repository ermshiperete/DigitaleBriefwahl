// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace DigitaleBriefwahl.Utils
{
	public static class RegistryManager
	{
		public static IRegistry Registry { get; private set; } = new WindowsRegistry();

		public static void SetRegistryProvider(IRegistry registry)
		{
			Registry?.Dispose();
			Registry = registry;
		}

		public static void Release()
		{
			Registry?.Dispose();
		}
	}
}