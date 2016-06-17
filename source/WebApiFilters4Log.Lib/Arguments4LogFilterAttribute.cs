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
		const string MSG_LOG_ARGS = "ARGS";

		string FormatLogArguments = string.Empty;
		internal string[] _MonitoredTypes = null;
		ILog Logger = null;

		#region Properties

		/// <summary>
		/// LogLevel utilizado para registrar os argumentos de uma acao. Padrao "INFO"
		/// </summary>
		public LogLevel ArgumentsLogLevel
		{
			get
			{
				return _ArgumentsLogLevel;
			}
			set
			{
				_ArgumentsLogLevel = value;
			}
		}

		LogLevel _ArgumentsLogLevel = LogLevel.INFO;

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

		#endregion Properties

		/// <summary>
		/// Construtor do filtro utilizado para logar os argumentos
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="typesMonitored">Nome completo dos tipos a serem monitorados. Use "*" para todos</param>
		public Arguments4LogFilterAttribute(string loggerName, params string[] typesMonitored)
		{
			Logger = LogManager.GetLogger(loggerName);

			if (typesMonitored != null && typesMonitored.Length > 0)
				_MonitoredTypes = typesMonitored;
			else
				_MonitoredTypes = new string[] { "*" };

			ChangeFormatMessage();
		}

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

			Dictionary<string, string> logArgs = actionContext.ActionArguments.GetActionArguments(_MonitoredTypes);

			if (logArgs.Count > 0)
			{
				Dictionary<string, string> logContext = actionContext.GetLogContext();

				Logger.LogMessage(ArgumentsLogLevel, FormatLogArguments, logContext.ToJson(), logArgs.ToJson());
			}

			return base.OnActionExecutingAsync(actionContext, cancellationToken);
		}

		internal void ChangeFormatMessage()
		{
			FormatLogArguments = string.Concat("{0} ", ArgumentsMessage, " {1}");
		}
	}
}
