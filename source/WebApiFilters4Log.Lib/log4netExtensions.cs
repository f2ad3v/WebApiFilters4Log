
namespace WebApiFilters4Log
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http.Controllers;

	/// <summary>
	/// Extensoes para log4net
	/// </summary>
	public static class Log4NetExtensions
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
		/// Extensao para obter o LogLevel ativo no Logger
		/// </summary>
		/// <param name="logger">log4net.ILog</param>
		public static LogLevel GetLogLevel(this log4net.ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");

			if (logger.IsDebugEnabled) return LogLevel.DEBUG;

			if (logger.IsInfoEnabled) return LogLevel.INFO;

			if (logger.IsWarnEnabled) return LogLevel.WARN;

			if (logger.IsErrorEnabled) return LogLevel.ERROR;

			if (logger.IsFatalEnabled) return LogLevel.FATAL;

			return LogLevel.OFF;
		}

		/// <summary>
		/// Extensao para alterar o LogLevel do Logger
		/// </summary>
		/// <param name="logger">log4net.ILog</param>
		/// <param name="logLevel">LogLevel</param>
		public static void SetLogLevel(this log4net.ILog logger, LogLevel logLevel)
		{
			if (logger == null) throw new ArgumentNullException("logger");

			log4net.Core.Level level = log4net.Core.Level.Off;

			switch (logLevel)
			{
				case LogLevel.INFO:
					level = log4net.Core.Level.Info;
					break;
				case LogLevel.DEBUG:
					level = log4net.Core.Level.Debug;
					break;
				case LogLevel.WARN:
					level = log4net.Core.Level.Warn;
					break;
				case LogLevel.ERROR:
					level = log4net.Core.Level.Error;
					break;
				case LogLevel.FATAL:
					level = log4net.Core.Level.Fatal;
					break;
			}

			((log4net.Repository.Hierarchy.Logger)logger.Logger).Level = level;
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
			bool contextHasPersisted = actionContext.ContextHasPersisted();

			var context = actionContext.GetLogContext(contextHasPersisted);

			logger.LogMessage(logLevel, context, message, args);

			if (!contextHasPersisted && logLevel >= logger.GetLogLevel()) actionContext.SetContextAsPersisted();
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

			if (args != null && args.Any()) msg = string.Format(message, args);

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
