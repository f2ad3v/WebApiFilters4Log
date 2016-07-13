namespace WebApiFilters4Log
{
	using log4net;
	using System;

	/// <summary>
	/// Classe estatica utilizada para expor metodos do log4net
	/// </summary>
	public static class Log4NetTools
	{
		/// <summary>
		/// Obtem o LogLevel de um Logger
		/// </summary>
		/// <param name="loggerName">LoggerName do Logger</param>
		/// <returns>LogLevel</returns>
		/// <exception cref="ArgumentException">Levanta uma excecao do tipo ArgumentException caso o logger nao exista</exception>
		public static LogLevel GetLevel(string loggerName)
		{
			var logger = LogManager.Exists(loggerName);

			if (logger == null) throw new ArgumentException("Logger not found!");

			return logger.GetLogLevel();
		}

		/// <summary>
		/// Altera o LogLevel de um Logger
		/// </summary>
		/// <param name="loggerName">LoggerName do Logger</param>
		/// <param name="level">LogLevel</param>
		/// <exception cref="ArgumentException">Levanta uma excecao do tipo ArgumentException caso o logger nao exista</exception>
		public static void SetLevel(string loggerName, LogLevel level)
		{
			var logger = LogManager.Exists(loggerName);

			if (logger == null) throw new ArgumentException("Logger not found!");

			logger.SetLogLevel(level);
		}
	}
}
