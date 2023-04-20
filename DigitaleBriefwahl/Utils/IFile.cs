// Copyright (c) 2023 Eberhard Beilharz
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace DigitaleBriefwahl.Utils
{
	public interface IFile
	{
		bool Exists(string path);
	}
}