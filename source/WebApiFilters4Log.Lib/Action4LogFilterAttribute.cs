namespace WebApiFilters4Log
{
	using log4net;
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web.Http.Controllers;
	using System.Web.Http.Filters;

	/// <summary>
	/// Filtro para logar o inicio e o fim de execucao de uma acao utilizando o log4net
	/// </summary>
	public class Action4LogFilterAttribute : ActionFilterAttribute
	{
		const string CONTEXT_ID = "ContextId";
		const string MSG_STARTING_ACTION = "Starting Action";
		const string MSG_END_ACTION = "End Action";
		const string MSG_LOG_ARGS = "ARGS";
		readonly ILog Logger;
		readonly LogLevel ActionLogLevel = LogLevel.DEBUG;
		string FormatLogArguments = string.Empty;

		#region Properties

		/// <summary>
		/// Tempo em segundos. Quando definido e se o tempo para o termino da execucao ultrapassa-lo o LogLevel será alterado para WARN ao registrar o fim da acao. -1 para desabilitar (padrao)
		/// </summary>
		public int TimeOutWarn { get; set; } = -1;

		/// <summary>
		/// Mensagem utilizada no registro de log para o inicio da execucao de uma acao. Valor padrao: Starting Action
		/// </summary>
		public string MessageStartingAction { get; set; } = MSG_STARTING_ACTION;

		/// <summary>
		/// Mensagem utilizada no registro de log para o fim da execucao de uma acao. Valor padrao: End Action
		/// </summary>
		public string MessageEndAction { get; set; } = MSG_END_ACTION;

		/// <summary>
		/// LogLevel utilizado para registrar os argumentos de uma acao. Padrao "INFO"
		/// </summary>
		public LogLevel ArgumentsLogLevel { get; set; } = LogLevel.INFO;
		
		/// <summary>
		/// Mensagem de separacao do contexto e os argumentos. Padrao "ARGS" - Ex: {CONTEXTO} ARGS {ARGUMENTOS}
		/// </summary>
		public string ArgumentsMessage
		{
			get
			{
				return _ArgumentsMessage;
			}
			set
			{
				_ArgumentsMessage = value;
				ChangeFormatMessage();
			}
		}

		string _ArgumentsMessage = MSG_LOG_ARGS;

		/// <summary>
		/// Lista de nome completo dos tipos a serem monitorados separados por ponto e virgula ';'. Use "*" para todos. Ex: "System.Int32;System.String"
		/// </summary>
		public string MonitoredTypes { get; set; } = string.Empty;


		#endregion Properties

		/// <summary>
		/// Construtor para Action4LogFilter
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="logLevel">LogLevel utilizado para registrar o inicio e em caso de sucesso o fim da execucao de uma acao. Caso termine em excecao sera registrado como ERROR. Padrao=DEBUG</param>
		/// <param name="timeOutWarn">Tempo em segundos. Quando definido e se o tempo para o termino da execucao ultrapassa-lo o LogLevel será alterado para WARN ao registrar o fim da acao. -1 para desabilitar (padrao)</param>
		public Action4LogFilterAttribute(string loggerName, LogLevel logLevel, int timeOutWarn)
		{
			if (!string.IsNullOrWhiteSpace(loggerName))
			{
				Logger = LogManager.GetLogger(loggerName);
			}

			ActionLogLevel = logLevel;

			TimeOutWarn = timeOutWarn;

			ChangeFormatMessage();
		}

		/// <summary>
		/// Construtor para Action4LogFilter
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="logLevel">LogLevel utilizado para registrar o inicio e em caso de sucesso o fim da execucao de uma acao. Caso termine em excecao sera registrado como ERROR. Padrao=DEBUG</param>
		public Action4LogFilterAttribute(string loggerName, LogLevel logLevel) : this(loggerName, logLevel, -1) { }

		/// <summary>
		/// Construtor para Action4LogFilter. Assume LogLevel.DEBUG
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		public Action4LogFilterAttribute(string loggerName) : this(loggerName, LogLevel.DEBUG) { }

		/// <summary>
		/// Construtor para Action4LogFilter. Assume LoggerName=CONTROLLER.ACTION e LogLevel.DEBUG
		/// </summary>
		public Action4LogFilterAttribute() : this(null, LogLevel.DEBUG) { }

		/// <summary>
		/// OnActionExecutingAsync executado antes da action
		/// </summary>
		/// <param name="actionContext">Instancia de HttpActionContext</param>
		/// <param name="cancellationToken">Instancia de CancellationToken</param>
		/// <returns>Task</returns>
		public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			if (actionContext.ActionDescriptor.IgnoreFilters())
				return base.OnActionExecutingAsync(actionContext, cancellationToken);

			var contextId = Guid.NewGuid().ToString();

			if (!actionContext.Request.Headers.Contains(CONTEXT_ID))
				actionContext.Request.Headers.Add(CONTEXT_ID, contextId);

			ThreadContext.Properties[CONTEXT_ID] = contextId;

			actionContext.Request.Headers.Date = DateTimeOffset.Now;

			ILog logger = GetLogger(actionContext.ActionDescriptor);

			if (logger.IsEnabled(ActionLogLevel))
			{
				logger.LogMessage(ActionLogLevel, actionContext, MessageStartingAction);

				if (ActionLogLevel >= logger.GetLogLevel()) actionContext.SetContextAsPersisted();
			}

			ArgumentsLog(actionContext);

			return base.OnActionExecutingAsync(actionContext, cancellationToken);
		}

		/// <summary>
		/// OnActionExecutedAsync executado apos a action
		/// </summary>
		/// <param name="actionExecutedContext">Instancia de HttpActionExecutedContext</param>
		/// <param name="cancellationToken">Instancia de CancellationToken</param>
		/// <returns>Task</returns>
		public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
		{
			if (actionExecutedContext.ActionContext.ActionDescriptor.IgnoreFilters())
				return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);

			ILog logger = GetLogger(actionExecutedContext.ActionContext.ActionDescriptor);

			ProcessLogActionExecuted(actionExecutedContext, logger);

			return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
		}

		private void ArgumentsLog(HttpActionContext actionContext)
		{
			var monitoredTypes = !string.IsNullOrWhiteSpace(MonitoredTypes) ? MonitoredTypes.Split(';') : new string[0];

			Dictionary<string, string> logArgs = actionContext.ActionArguments.GetActionArguments(monitoredTypes);

			if (logArgs.Count > 0 && Logger.IsEnabled(ArgumentsLogLevel))
			{
				bool contextHasPersisted = actionContext.ContextHasPersisted();

				Dictionary<string, string> logContext = actionContext.GetLogContext(contextHasPersisted);

				Logger.LogMessage(ArgumentsLogLevel, FormatLogArguments, logContext.ToJson(), logArgs.ToJson());
			}
		}

		private void ProcessLogActionExecuted(HttpActionExecutedContext actionExecutedContext, ILog logger)
		{
			bool contextHasPersisted = actionExecutedContext.ActionContext.ContextHasPersisted();

			var logContext = actionExecutedContext.ActionContext.GetLogContext(contextHasPersisted);

			bool warn = false;

			if (actionExecutedContext.Request.Headers.Date.HasValue)
			{
				DateTimeOffset dtRequest = actionExecutedContext.Request.Headers.Date.Value;

				var ts = DateTimeOffset.Now.Subtract(dtRequest);

				warn = (TimeOutWarn >= 0 && ts.TotalSeconds > TimeOutWarn) ? true : false;

				if (warn) logContext.Add("WarnTimeout", "true");

				logContext.Add("Time", ts.TotalSeconds.ToString("#0.0000"));
			}

			if (actionExecutedContext.Response != null)
			{
				logContext.Add("StatusCode", string.Format("{0}({1})", actionExecutedContext.Response.StatusCode.ToString(), (int)actionExecutedContext.Response.StatusCode));
			}

			LogLevel level = warn ? LogLevel.WARN : ActionLogLevel;

			if (actionExecutedContext.Exception != null)
			{
				logContext["ErrorMessage"] = actionExecutedContext.Exception.Message;

				logContext["ErrorType"] = actionExecutedContext.Exception.GetType().Name;

				level = LogLevel.ERROR;
			}

			logger.LogMessage(level, "{0} - {1}", logContext.ToJson(), MessageEndAction);
		}

		private ILog GetLogger(HttpActionDescriptor actionDescriptor)
		{
			if (Logger != null) return Logger;

			return actionDescriptor.GetLogger();
		}

		internal void ChangeFormatMessage()
		{
			FormatLogArguments = string.Concat("{0} ", ArgumentsMessage, " {1}");
		}
	}
}
