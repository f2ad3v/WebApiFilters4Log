namespace WebApiFilters4Log
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http.Controllers;

	/// <summary>
	/// Extensoes para log4net
	/// </summary>
	public static class log4netExtensions
	{
		/// <summary>
		/// Extensao para identificar o LogLevel ativo
		/// </summary>
		/// <param name="logger">log4net.ILog</param>
		/// <param name="logLevel">LogLevel</param>
		/// <returns>true ou false</returns>
		public static bool IsEnabled(this log4net.ILog logger, LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.INFO:
					return logger.IsInfoEnabled;

				case LogLevel.WARN:
					return logger.IsWarnEnabled;

				case LogLevel.ERROR:
					return logger.IsErrorEnabled;

				case LogLevel.FATAL:
					return logger.IsFatalEnabled;

				default:
					return logger.IsDebugEnabled;
			}
		}

		/// <summary>
		/// Extensao para logar utilizando um LogLevel
		/// </summary>
		/// <param name="logger">log4net.ILog</param>
		/// <param name="logLevel">LogLevel</param>
		/// <param name="message">Mensagem</param>
		/// <param name="args">Argumentos opcionais utilizados na formatacao da mensagem</param>
		public static void LogMessage(this log4net.ILog logger, LogLevel logLevel, string message, params string[] args)
		{
			if (logger == null) throw new ArgumentNullException("logger");

			switch (logLevel)
			{
				case LogLevel.INFO:
					if (args == null || args.Length.Equals(0)) logger.Info(message);
					else logger.InfoFormat(message, args);
					break;

				case LogLevel.WARN:
					if (args == null || args.Length.Equals(0)) logger.Warn(message);
					else logger.WarnFormat(message, args);
					break;

				case LogLevel.ERROR:
					if (args == null || args.Length.Equals(0)) logger.Error(message);
					else logger.ErrorFormat(message, args);
					break;

				case LogLevel.FATAL:
					if (args == null || args.Length.Equals(0)) logger.Fatal(message);
					else logger.FatalFormat(message, args);
					break;

				default:
					if (args == null || args.Length.Equals(0)) logger.Debug(message);
					else logger.DebugFormat(message, args);
					break;
			}
		}

		/// <summary>
		/// Extensao para logar utilizando um LogLevel e extraindo o contexto do HttpActionContext
		/// </summary>
		/// <param name="logger">log4net.ILog</param>
		/// <param name="logLevel">LogLevel</param>
		/// <param name="actionContext">Instancia de HttpActionContext</param>
		/// <param name="message">Mensagem</param>
		/// <param name="args">Argumentos opcionais utilizados na formatacao da mensagem</param>
		public static void LogMessage(this log4net.ILog logger, LogLevel logLevel, HttpActionContext actionContext, string message, params string[] args)
		{
			var context = actionContext.GetLogContext();

			logger.LogMessage(logLevel, context, message, args);
		}

		/// <summary>
		/// Extensao para logar utilizando um LogLevel
		/// </summary>
		/// <param name="logger">log4net.ILog</param>
		/// <param name="logLevel">LogLevel</param>
		/// <param name="context">Dicionario de contexto</param>
		/// <param name="message">Mensagem</param>
		/// <param name="args">Argumentos opcionais utilizados na formatacao da mensagem</param>
		public static void LogMessage(this log4net.ILog logger, LogLevel logLevel, Dictionary<string, string> context, string message, params string[] args)
		{
			var msg = message;

			if (args != null && args.Count() > 0) msg = string.Format(message, args);

			if (context != null && context.Count > 0) msg = string.Format("{0} - {1}", context.ToJson(), msg);

			logger.LogMessage(logLevel, msg);
		}

		/// <summary>
		/// Extensao para logar uma excecao em formato json
		/// </summary>
		/// <param name="logger">log4net.ILog</param>
		/// <param name="exception">Instancia de System.Exception</param>
		public static void LogMessage(this log4net.ILog logger, Exception exception)
		{
			var errorId = Guid.NewGuid();

			if (exception is CommonException) errorId = (exception as CommonException).Id;

			var exceptionObj = exception.ToDynamicObject(errorId);

			var jsonException = Newtonsoft.Json.JsonConvert.SerializeObject(exceptionObj);

			logger.LogMessage(LogLevel.ERROR, jsonException);
		}
	}
}
