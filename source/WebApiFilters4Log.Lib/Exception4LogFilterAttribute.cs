namespace WebApiFilters4Log
{
	using Newtonsoft.Json;
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web.Http;
	using System.Web.Http.Filters;

	/// <summary>
	/// Filtro para logar excecoes utilizando o log4net e tratar o resultado adequadamente
	/// </summary>
	public class Exception4LogFilterAttribute : ExceptionFilterAttribute
	{
		private const string HEADER_DEBUG = "X-DebugError";
		private const string DATE_TIME_FORMAT = "dd/MM/yyyy HH:mm:ss.fffff";

		private readonly string _LoggerName = string.Empty;
		private readonly string _DebugKey = string.Empty;
		private readonly string _HeaderName = HEADER_DEBUG;

		/// <summary>
		/// Construtor do filtro utilizado para logar excecoes e tratar o resultado adequadamente
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="debugKey">Define uma chave de confianca para liberar acesso ao resultado com detalhes da excecao. Caso nao informado utilize o valor "true" para o cabecalho "X-DebugError"</param>
		/// <param name="headerName">Define um nome para o cabecalho responsavel por liberar acesso ao resultado com detalhes da excecao. Caso nao informado sera utilizado o nome "X-DebugError"</param>
		public Exception4LogFilterAttribute(string loggerName, string debugKey, string headerName = HEADER_DEBUG)
		{
			_LoggerName = loggerName;
			_DebugKey = debugKey;
			_HeaderName = headerName;
		}

		/// <summary>
		/// Construtor do filtro utilizado para logar excecoes e tratar o resultado adequadamente
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		public Exception4LogFilterAttribute(string loggerName) : this(loggerName, string.Empty)
		{
		}

		/// <summary>
		/// OnExceptionAsync executado quando uma excecao for lancada na action
		/// </summary>
		/// <param name="actionExecutedContext">Instancia de HttpActionExecutedContext</param>
		/// <param name="cancellationToken">Instancia de CancellationToken</param>
		/// <returns>Task</returns>
		public override Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
		{
			var logContext = actionExecutedContext.ActionContext.GetLogContext();

			log4net.ILog logger = log4net.LogManager.GetLogger(_LoggerName);

			Guid errorId = Guid.NewGuid();
			DateTime dtError = DateTime.Now;

			if (actionExecutedContext.Exception is ControllerException)
			{
				errorId = (actionExecutedContext.Exception as ControllerException).Id;
				dtError = (actionExecutedContext.Exception as ControllerException).DateTimeError;
			}

			var logObj = actionExecutedContext.Exception.ToDynamicObject(errorId);

			string jsonMsg = JsonConvert.SerializeObject(logObj);

			logger.Error(jsonMsg);

			bool returnErrorDetail = false;

			if (actionExecutedContext.Request.Headers.Contains(_HeaderName))
			{
				var values = actionExecutedContext.Request.Headers.GetValues(_HeaderName).Select(v => v.ToLower()).ToList();

				returnErrorDetail = (string.IsNullOrWhiteSpace(_DebugKey) && values.Contains("true")) 
					|| (!string.IsNullOrWhiteSpace(_DebugKey) && values.Contains(_DebugKey.ToLower()));
			}

			if (returnErrorDetail) 
			{
				actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new JsonContent(logObj),
					RequestMessage = actionExecutedContext.Request
				};
			}
			else
			{
				HttpError httpError = new HttpError(actionExecutedContext.Exception.Message) { { "ErrorCode", 500 }, { "ErrorId", errorId.ToString() }, { "DateTime", dtError.ToString(DATE_TIME_FORMAT) } };

				actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, httpError);
			}

			return base.OnExceptionAsync(actionExecutedContext, cancellationToken);
		}
	}
}
