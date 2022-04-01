// Copyright (c) 2018 Eberhard Beilharz
// This software is licensed under the GNU General Public License version 3
// (https://opensource.org/licenses/GPL-3.0)

namespace DigitaleBriefwahl.ExceptionHandling
{
	public static class Logger
	{
		private static BasicLogger _logger;

		public static string LogDirectory => _logger.LogDirectory;

		static Logger()
		{
			_logger = new BasicLogger();
		}

		public static void CreateLogger(BasicLogger logger)
		{
			_logger = logger;
		}

		public static string LogFile => _logger.LogFile;

		public static void Log(string text)
		{
			_logger.Log(text);
		}

		public static void Error(string text)
		{
			_logger.Error(text);
		}

		public static void Truncate()
		{
			_logger.Truncate();
		}

		public static string GetLogSinceLastStart()
		{
			return _logger.GetLogSinceLastStart();
		}
	}
}