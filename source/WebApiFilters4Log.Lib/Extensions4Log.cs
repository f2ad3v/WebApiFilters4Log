namespace WebApiFilters4Log
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http.Controllers;

	/// <summary>
	/// Extensoes
	/// </summary>
	public static class Extensions4Log
	{
		const string CONTEXT_ID = "ContextId";

		/// <summary>
		/// Extensao para extrair do HttpActionContext um dicionario com os detalhes da acao
		/// </summary>
		/// <param name="actionContext">HttpActionDescriptor</param>
		/// <param name="onlyContextId">true para retornar somente o id do contexto e false para retornar tudo</param>
		/// <returns>dicionario com os detalhes da acao</returns>
		public static Dictionary<string, string> GetLogContext(this HttpActionContext actionContext, bool onlyContextId = false)
		{
			var contextId = string.Empty;

			if (actionContext.Request.Headers.Contains(CONTEXT_ID))
				contextId = actionContext.Request.Headers.GetValues(CONTEXT_ID).SingleOrDefault();

			if (onlyContextId) return new Dictionary<string, string> { { CONTEXT_ID, contextId } };

			string controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;

			string actionName = actionContext.ActionDescriptor.ActionName;

			Dictionary<string, string> context = new Dictionary<string, string>();

			if (!string.IsNullOrWhiteSpace(contextId)) context.Add(CONTEXT_ID, contextId);

			context.Add("MachineName", Environment.MachineName);
			context.Add("Controller", controllerName);
			context.Add("Action", actionName);
			context.Add("Method", actionContext.Request.Method.Method);

			if (actionContext.RequestContext.Principal != null && actionContext.RequestContext.Principal.Identity != null)
			{
				context.Add("IsAuthenticated", actionContext.RequestContext.Principal.Identity.IsAuthenticated.ToString());

				if (!string.IsNullOrWhiteSpace(actionContext.RequestContext.Principal.Identity.Name))
					context.Add("UserName", actionContext.RequestContext.Principal.Identity.Name);
			}

			return context;
		}

		/// <summary>
		/// Extensao para obter uma instancia de log4net.ILog
		/// </summary>
		/// <param name="actionDescriptor">HttpActionDescriptor</param>
		/// <returns>Retorna um logger correspondente ao nome completo da acao</returns>
		internal static log4net.ILog GetLogger(this HttpActionDescriptor actionDescriptor)
		{
			return log4net.LogManager.GetLogger(actionDescriptor.GetFullActionName());
		}

		/// <summary>
		/// Extensao para obter o nome completo da acao
		/// </summary>
		/// <param name="actionDescriptor">HttpActionDescriptor</param>
		/// <returns>Retorna o nome completo da acao. Ex: Controller.Action</returns>
		internal static string GetFullActionName(this HttpActionDescriptor actionDescriptor)
		{
			return string.Format("{0}.{1}", actionDescriptor.ControllerDescriptor.ControllerName, actionDescriptor.ActionName);
		}

		/// <summary>
		/// Extensao para obter um dicionario contendo os argumentos passados para uma acao
		/// </summary>
		/// <param name="arguments">IDictionary</param>
		/// <param name="types">Nome completo dos tipos que serao retornados</param>
		/// <returns>Retorna um dicionario contendo os argumentos serializados em json</returns>
		internal static Dictionary<string, string> GetActionArguments(this IDictionary<string, object> arguments, params string[] types)
		{
			Dictionary<string, string> context = new Dictionary<string, string>();

			if (arguments == null || arguments.Count == 0 || types == null || types.Length == 0) return context;

			foreach (KeyValuePair<string, object> kvp in arguments)
			{
				if (!types.Contains("*") && (kvp.Value == null || (kvp.Value != null && !types.Contains(kvp.Value.GetType().FullName)))) continue;

				string vlr = GetValue(kvp);

				string key = kvp.Key;

				if (kvp.Value != null) key = string.Format("{0} {1}", kvp.Value.GetType().FullName, kvp.Key);

				context.Add(key, vlr);
			}

			return context;
		}

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
		/// Extensao para obter um dicionario contendo detalhes da excecao
		/// </summary>
		/// <param name="ce">ControllerException</param>
		/// <returns>Retorna um dicionario contendo os detalhes da excecao</returns>
		internal static Dictionary<string, string> GetAdditionalInfo(this ControllerException ce)
		{
			var addInfo = new Dictionary<string, string>
			{
				{ "Controller", ce.Controller },
				{ "Action", ce.Action },
				{ "PathAndQuery", ce.PathAndQuery },
				{ "User", ce.User },
				{ "ContextId", ce.ContextId.ToString() },
			};

			if (ce.Arguments != null && ce.Arguments.Count > 0)
			{
				foreach (KeyValuePair<string, string> kvp in ce.Arguments) addInfo.Add(kvp.Key, kvp.Value);
			}

			return addInfo;
		}

		/// <summary>
		/// Extensao para obter um objeto dinamico utilizando como base a excecao
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <param name="errorId">Id da excecao</param>
		/// <returns>Retorna um objeto dinamico com os detalhes da excecao</returns>
		internal static object ToDynamicObject(this Exception ex, Guid errorId)
		{
			DateTime dateTime = (ex is CommonException) ? (ex as CommonException).DateTimeError : DateTime.Now;

			var exLog = new
			{
				Id = errorId,
				DateTime = dateTime,
				Message = ex.Message,
				Source = ex.Source,
				StackTrace = ProcessStackTrace(ex.StackTrace),
				ExceptionType = ex.GetType().FullName,
				AdditionalInfo = new Dictionary<string, string>(),
				InnerException = ex.InnerException != null ? ex.InnerException.ToDynamicObject(errorId) : null
			};

			if (ex is ControllerException)
			{
				var addInfo = (ex as ControllerException).GetAdditionalInfo();

				foreach (KeyValuePair<string, string> kvp in addInfo) exLog.AdditionalInfo.Add(kvp.Key, kvp.Value);
			}

			return exLog;
		}

		/// <summary>
		/// Extensao para converter um dicionario para json
		/// </summary>
		/// <param name="dic">Instancia de Dictionary</param>
		/// <returns>json</returns>
		public static string ToJson(this Dictionary<string, string> dic)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(dic);
		}

		#region private methods

		private static string ProcessStackTrace(string stackTrace)
		{
			if (!stackTrace.Contains("lambda_method(Closure")) return stackTrace;

			var firstPart = stackTrace.Substring(0, stackTrace.IndexOf("lambda_method(Closure"));

			return firstPart.Substring(0, firstPart.LastIndexOf("\r\n"));
		}

		private static string GetValue(KeyValuePair<string, object> kvp)
		{
			if (kvp.Value == null) return "NULL";

			try
			{
				return Newtonsoft.Json.JsonConvert.SerializeObject(kvp.Value).Replace("\"", "'");
			}
			catch
			{
				return "NOT SERIALIZABLE";
			}
		}

		#endregion private methods
	}
}
