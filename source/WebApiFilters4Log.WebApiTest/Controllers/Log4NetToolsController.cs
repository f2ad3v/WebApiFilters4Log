using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiFilters4Log.WebApiTest.Controllers
{
	public class Log4NetToolsController : ApiController
	{
		[HttpGet]
		[Action4LogFilter("Log4NetTools", LogLevel.DEBUG)]
		public IHttpActionResult GetLogLevel_OFF_Success()
		{
			var loggerName = "Log4NetTools";
			var logger = log4net.LogManager.GetLogger(loggerName);

			var level = Log4NetTools.GetLevel(loggerName);

			if (level != LogLevel.OFF) throw new InvalidOperationException("OFF fail");

			logger.LogMessage(LogLevel.FATAL, ActionContext, "FATAL - Teste sem log");

			level = LogLevel.FATAL;

			Log4NetTools.SetLevel(loggerName, level);

			logger.LogMessage(LogLevel.FATAL, ActionContext, "FATAL - Teste com log");
			logger.LogMessage(LogLevel.ERROR, ActionContext, "ERROR - Teste sem log");

			level = Log4NetTools.GetLevel(loggerName);

			if (level != LogLevel.FATAL) throw new InvalidOperationException("FATAL fail");

			level = LogLevel.ERROR;

			Log4NetTools.SetLevel(loggerName, level);

			logger.LogMessage(LogLevel.ERROR, ActionContext, "ERROR - Teste com log");
			logger.LogMessage(LogLevel.WARN, ActionContext, "WARN, - Teste sem log");

			level = Log4NetTools.GetLevel(loggerName);

			if (level != LogLevel.ERROR) throw new InvalidOperationException("ERROR fail");

			level = LogLevel.WARN;

			Log4NetTools.SetLevel(loggerName, level);

			logger.LogMessage(LogLevel.WARN, ActionContext, "WARN - Teste com log");
			logger.LogMessage(LogLevel.INFO, ActionContext, "INFO, - Teste sem log");

			level = Log4NetTools.GetLevel(loggerName);

			if (level != LogLevel.WARN) throw new InvalidOperationException("WARN fail");
			
			level = LogLevel.INFO;

			Log4NetTools.SetLevel(loggerName, level);

			logger.LogMessage(LogLevel.INFO, ActionContext, "INFO - Teste com log");
			logger.LogMessage(LogLevel.DEBUG, ActionContext, "DEBUG, - Teste sem log");

			level = Log4NetTools.GetLevel(loggerName);

			if (level != LogLevel.INFO) throw new InvalidOperationException("WARN fail");

			level = LogLevel.DEBUG;

			Log4NetTools.SetLevel(loggerName, level);

			logger.LogMessage(LogLevel.DEBUG, ActionContext, "DEBUG - Teste com log");

			level = Log4NetTools.GetLevel(loggerName);

			if (level != LogLevel.DEBUG) throw new InvalidOperationException("DEBUG fail");

			return Ok(level.ToString());
		}

		[HttpGet]
		[Action4LogFilter("Log4NetTools")]
		public IHttpActionResult GetLogLevel_Fail()
		{
			log4net.ILog logger = null;

			try
			{
				logger.GetLogLevel();
			}
			catch (ArgumentException)
			{
				Console.WriteLine("...");
			}

			try
			{
				logger.SetLogLevel(LogLevel.DEBUG);
			}
			catch (ArgumentException)
			{
				Console.WriteLine("...");
			}

			try
			{
				Log4NetTools.SetLevel("Invalid", LogLevel.DEBUG);
			}
			catch (ArgumentException)
			{
				Console.WriteLine("...");
			}

			return Ok(Log4NetTools.GetLevel("Invalid"));
		}
	}
}
