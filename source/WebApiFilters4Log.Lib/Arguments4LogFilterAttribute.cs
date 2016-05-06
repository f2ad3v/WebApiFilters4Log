namespace WebApiFilters4Log
{
	using log4net;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web.Http.Controllers;
	using System.Web.Http.Filters;

	/// <summary>
	/// Filtro para logar os argumentos de uma acao utilizando o log4net
	/// </summary>
	public class Arguments4LogFilterAttribute : ActionFilterAttribute
	{
		private const string MSG_LOG_ARGS = "{0} ARGS {1}"; // {0}=Chaves de contexto / {1}=Argumentos

		private string[] _MonitoredTypes = null;
		private readonly LogLevel _LogLevel = LogLevel.DEBUG;
		private readonly ILog _Logger = null;

		/// <summary>
		/// Construtor do filtro utilizado para logar os argumentos
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="logLevel">Log Level</param>
		/// <param name="typesMonitored">Nome completo dos tipos a serem monitorados. Use "*" para todos</param>
		public Arguments4LogFilterAttribute(string loggerName, LogLevel logLevel, params string[] typesMonitored)
		{
			_LogLevel = logLevel;
			_Logger = LogManager.GetLogger(loggerName);
			_MonitoredTypes = typesMonitored;
		}

		/// <summary>
		/// Construtor do filtro utilizado para logar os argumentos
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		public Arguments4LogFilterAttribute(string loggerName) : this(loggerName, LogLevel.DEBUG, "*") { }

		/// <summary>
		/// OnActionExecutingAsync executado antes da action
		/// </summary>
		/// <param name="actionContext">Instancia de HttpActionContext</param>
		/// <param name="cancellationToken">Instancia de CancellationToken</param>
		/// <returns>Task</returns>
		public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			Dictionary<string, string> logArgs = actionContext.ActionArguments.GetActionArguments(_MonitoredTypes);

			if (logArgs.Count > 0)
			{
				Dictionary<string, string> logContext = actionContext.GetLogContext();

				_Logger.LogMessage(_LogLevel, MSG_LOG_ARGS, logContext.ToJson(), logArgs.ToJson());
			}

			return base.OnActionExecutingAsync(actionContext, cancellationToken);
		}
	}
}
