// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Linq;
using DigitaleBriefwahl.Utils;

namespace DigitaleBriefwahlTests.Utils
{
	public class InMemoryRegistryKey: IRegistryKey
	{
		private Dictionary<string, InMemoryRegistryKey> _children = new Dictionary<string, InMemoryRegistryKey>();
		private Dictionary<string, object>              _values   = new Dictionary<string, object>();

		public void Reset()
		{
			_children.Clear();
			_values.Clear();
		}

		public IRegistryKey CreateKey(string path, string key, string value)
		{
			if (string.IsNullOrEmpty(path))
			{
				_values[key] = value;
				return this;
			}

			var (first, rest) = SplitFirstPart(path);
			if (!_children.TryGetValue(first, out var registryKey))
			{
				registryKey = new InMemoryRegistryKey();
				_children.Add(first, registryKey);
				registryKey.CreateKey(rest, key, value);
			}

			return registryKey;
		}

		private static (string, string) SplitFirstPart(string name)
		{
			var index = name.IndexOf('\\');
			return index >= 0
				? (name.Substring(0, index), name.Substring(index + 1))
				: (name, null);
		}

		#region IRegistryKey implementation

		public void Dispose()
		{
			// nothing to do
		}

		public IRegistryKey OpenSubKey(string name)
		{
			var (first, rest) = SplitFirstPart(name);
			return _children[first]?.OpenSubKey(rest);
		}

		public object GetValue(string name) => _values[name];

		public int SubKeyCount => _children.Count;

		public string[] GetSubKeyNames() => _children.Keys.ToArray();

		#endregion
	}
}