namespace WebApiFilters4Log
{
	using log4net;
	using System;
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

		ILog _Logger = null;
		LogLevel _ActionLogLevel = LogLevel.DEBUG;

		#region Properties

		/// <summary>
		/// Tempo em segundos. Quando definido e se o tempo para o termino da execucao ultrapassa-lo o LogLevel será alterado para WARN ao registrar o fim da acao. -1 para desabilitar (padrao)
		/// </summary>
		public int TimeOutWarn
		{
			get
			{
				return _TimeOutWarn;
			}

			set
			{
				_TimeOutWarn = value;
			}
		}

		int _TimeOutWarn = -1;

		/// <summary>
		/// Mensagem utilizada no registro de log para o inicio da execucao de uma acao. Valor padrao: Starting Action
		/// </summary>
		public string MessageStartingAction
		{
			get
			{
				return _MessageStartingAction;
			}

			set
			{
				_MessageStartingAction = value;
			}
		}

		string _MessageStartingAction = MSG_STARTING_ACTION;

		/// <summary>
		/// Mensagem utilizada no registro de log para o fim da execucao de uma acao. Valor padrao: End Action
		/// </summary>
		public string MessageEndAction
		{
			get
			{
				return _MessageEndAction;
			}

			set
			{
				_MessageEndAction = value;
			}
		}

		string _MessageEndAction = MSG_END_ACTION;

		#endregion Properties

		/// <summary>
		/// Construtor para Action4LogFilter
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="logLevel">LogLevel utilizado para registrar o inicio e em caso de sucesso o fim da execucao de uma acao. Caso termine em excecao sera registrado como ERROR. Padrao=DEBUG</param>
		public Action4LogFilterAttribute(string loggerName, LogLevel logLevel)
		{
			if (!string.IsNullOrWhiteSpace(loggerName))
			{
				_Logger = LogManager.GetLogger(loggerName);
			}

			_ActionLogLevel = logLevel;
		}

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
			if (!actionContext.Request.Headers.Contains(CONTEXT_ID))
				actionContext.Request.Headers.Add(CONTEXT_ID, Guid.NewGuid().ToString());

			actionContext.Request.Headers.Date = DateTimeOffset.Now;

			ILog logger = GetLogger(actionContext.ActionDescriptor);

			if (logger.IsEnabled(_ActionLogLevel))
			{
				logger.LogMessage(_ActionLogLevel, actionContext, MessageStartingAction);
			}

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
			ILog logger = GetLogger(actionExecutedContext.ActionContext.ActionDescriptor);

			ProcessLogActionExecuted(actionExecutedContext, logger, _ActionLogLevel);

			return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
		}

		private ILog GetLogger(HttpActionDescriptor actionDescriptor)
		{
			if (_Logger != null) return _Logger;

			return actionDescriptor.GetLogger();
		}

		private void ProcessLogActionExecuted(HttpActionExecutedContext actionExecutedContext, ILog logger, LogLevel logLevel)
		{
			var logContext = actionExecutedContext.ActionContext.GetLogContext(true);

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

			LogLevel level = (actionExecutedContext.Exception == null) ? (warn ? LogLevel.WARN : _ActionLogLevel) : LogLevel.ERROR;

			logger.LogMessage(level, "{0} - {1}", logContext.ToJson(), MessageEndAction);
		}
	}
}
