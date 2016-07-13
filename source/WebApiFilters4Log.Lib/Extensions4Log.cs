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
		const string PERSISTED_CONTEXT = "PersistedContext";

		/// <summary>
		/// Extensao para extrair do HttpActionContext um dicionario com os detalhes da acao
		/// </summary>
		/// <param name="actionContext">HttpActionDescriptor</param>
		/// <returns>dicionario com os detalhes da acao</returns>
		public static Dictionary<string, string> GetLogContext(this HttpActionContext actionContext)
		{
			return actionContext.GetLogContext(false);
		}

		/// <summary>
		/// Extensao para extrair do HttpActionContext um dicionario com os detalhes da acao
		/// </summary>
		/// <param name="actionContext">HttpActionDescriptor</param>
		/// <param name="onlyContextId">true para retornar somente o id do contexto e false para retornar tudo</param>
		/// <returns>dicionario com os detalhes da acao</returns>
		public static Dictionary<string, string> GetLogContext(this HttpActionContext actionContext, bool onlyContextId)
		{
			var contextId = string.Empty;

			if (actionContext.Request.Headers.Contains(CONTEXT_ID))
				contextId = actionContext.Request.Headers.GetValues(CONTEXT_ID).SingleOrDefault();

			//if (actionContext.ContextHasPersisted()) 
			if (onlyContextId) return new Dictionary<string, string> { { CONTEXT_ID, contextId } };

			string controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;

			string actionName = actionContext.ActionDescriptor.ActionName;

			Dictionary<string, string> context = new Dictionary<string, string>();

			if (!string.IsNullOrWhiteSpace(contextId)) context.Add(CONTEXT_ID, contextId);

			context.Add("MachineName", Environment.MachineName);
			context.Add("Controller", controllerName);
			context.Add("Action", actionName);
			context.Add("Method", actionContext.Request.Method.Method);

			var userName = "Anonymous";

			if (actionContext.RequestContext.Principal != null && actionContext.RequestContext.Principal.Identity != null)
			{
				context.Add("IsAuthenticated", actionContext.RequestContext.Principal.Identity.IsAuthenticated.ToString());

				if (!string.IsNullOrWhiteSpace(actionContext.RequestContext.Principal.Identity.Name))
					userName = actionContext.RequestContext.Principal.Identity.Name;
			}

			context.Add("UserName", userName);

			return context;
		}

		/// <summary>
		/// Verifica se o contexto ja foi persistido
		/// </summary>
		/// <param name="actionContext">HttpActionDescriptor</param>
		/// <returns>true=persistido; false=nao persistido</returns>
		internal static bool ContextHasPersisted(this HttpActionContext actionContext)
		{
			return actionContext.Request.Headers.Contains(PERSISTED_CONTEXT)
				&& actionContext.Request.Headers.GetValues(PERSISTED_CONTEXT).SingleOrDefault() == "true";
		}

		/// <summary>
		/// Define o contexto como persistido
		/// </summary>
		/// <param name="actionContext">HttpActionDescriptor</param>
		internal static void SetContextAsPersisted(this HttpActionContext actionContext)
		{
			if (!actionContext.Request.Headers.Contains(PERSISTED_CONTEXT))
				actionContext.Request.Headers.Add(PERSISTED_CONTEXT, "true");
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
		/// Extensao para obter um dicionario contendo detalhes da excecao
		/// </summary>
		/// <param name="ce">ControllerException</param>
		/// <returns>Retorna um dicionario contendo os detalhes da excecao</returns>
		internal static Dictionary<string, string> GetAdditionalInfo(this ControllerException ce)
		{
			var addInfo = new Dictionary<string, string>
			{
				{ "MachineName", ce.MachineName },
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

		/// <summary>
		/// Extensao para verificar se existe o atributo IgnoreFilters associado a controller ou a action
		/// </summary>
		/// <param name="actionDescriptor">HttpActionDescriptor</param>
		/// <returns></returns>
		public static bool IgnoreFilters(this HttpActionDescriptor actionDescriptor)
		{
			return actionDescriptor.GetCustomAttributes<IgnoreLogAttribute>().Count > 0
				|| actionDescriptor.ControllerDescriptor.GetCustomAttributes<IgnoreLogAttribute>().Count > 0;
		}

		#region private methods

		private static string ProcessStackTrace(string stackTrace)
		{
			if (string.IsNullOrWhiteSpace(stackTrace)) return string.Empty;

			if (!stackTrace.Contains("lambda_method(Closure")) return stackTrace;

			var firstPart = stackTrace.Substring(0, stackTrace.IndexOf("lambda_method(closure", StringComparison.InvariantCultureIgnoreCase));

			return firstPart.Substring(0, firstPart.LastIndexOf("\r\n", StringComparison.InvariantCultureIgnoreCase));
		}

		private static string GetValue(KeyValuePair<string, object> kvp)
		{
			if (kvp.Value == null) return "NULL";

			if (kvp.Value.GetType().IsPrimitive || kvp.Value.GetType().FullName.Equals("system.string", StringComparison.InvariantCultureIgnoreCase))
				return kvp.Value.ToString();

			return Newtonsoft.Json.JsonConvert.SerializeObject(kvp.Value);
		}

		#endregion private methods
	}
}
