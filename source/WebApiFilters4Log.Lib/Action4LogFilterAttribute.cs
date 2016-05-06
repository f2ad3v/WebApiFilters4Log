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
		private const string MSG_INIT_ACTION = "Starting Action";
		private const string MSG_END_ACTION = "{0} - End Action"; // {0}=Chaves de contexto
		private readonly LogLevel _LogLevel = LogLevel.DEBUG;
		private ILog _Logger = null;

		/// <summary>
		/// Construtor do filtro utilizado para logar o inicio e o fim de execucao de uma acao
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="logLevel">Log Level</param>
		public Action4LogFilterAttribute(string loggerName, LogLevel logLevel)
		{
			if (!string.IsNullOrWhiteSpace(loggerName))
			{
				_Logger = LogManager.GetLogger(loggerName);
			}

			_LogLevel = logLevel;
		}

		/// <summary>
		/// Construtor do filtro utilizado para logar o inicio e o fim de execucao de uma acao
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

			if (logger.IsEnabled(_LogLevel))
			{
				logger.LogMessage(_LogLevel, actionContext, MSG_INIT_ACTION);
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

			if (logger.IsEnabled(_LogLevel))
			{
				ProcessLogActionExecuted(actionExecutedContext, logger, _LogLevel);
			}

			return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
		}

		private ILog GetLogger(HttpActionDescriptor actionDescriptor)
		{
			if (_Logger != null) return _Logger;

			return actionDescriptor.GetLogger();
		}

		private static void ProcessLogActionExecuted(HttpActionExecutedContext actionExecutedContext, ILog logger, LogLevel logLevel)
		{
			var logContext = actionExecutedContext.ActionContext.GetLogContext(true);

			if (actionExecutedContext.Request.Headers.Date.HasValue)
			{
				DateTimeOffset dtRequest = actionExecutedContext.Request.Headers.Date.Value;

				var ts = DateTimeOffset.Now.Subtract(dtRequest);

				logContext.Add("Time", ts.TotalSeconds.ToString("#0.0000"));
			}

			if (actionExecutedContext.Response != null)
			{
				logContext.Add("StatusCode", string.Format("{0}({1})", actionExecutedContext.Response.StatusCode.ToString(), (int)actionExecutedContext.Response.StatusCode));
			}

			LogLevel level = (actionExecutedContext.Exception == null) ? logLevel : LogLevel.ERROR;

			logger.LogMessage(level, MSG_END_ACTION, logContext.ToJson());
		}
	}
}
