namespace WebApiFilters4Log
{
	using log4net;
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
		const string HEADER_DEBUG = "X-DebugError";
		const string DATE_TIME_FORMAT = "dd/MM/yyyy HH:mm:ss.fffff";

		ILog Logger = null;

		#region Properties

		/// <summary>
		/// Define uma chave de confianca para liberar acesso ao resultado com detalhes da excecao. Caso nao informado utilize o valor "true" para o cabecalho "X-DebugError"
		/// </summary>
		public string DebugKey
		{
			get
			{
				return _DebugKey;
			}
			set
			{
				_DebugKey = value;
			}
		}

		string _DebugKey = string.Empty;

		/// <summary>
		/// Define um nome para o cabecalho responsavel por liberar acesso ao resultado com detalhes da excecao. Caso nao informado sera utilizado "X-DebugError"
		/// </summary>
		public string HeaderName
		{
			get
			{
				return _HeaderName;
			}
			set
			{
				_HeaderName = value;
			}
		}

		string _HeaderName = HEADER_DEBUG;

		#endregion Properties

		/// <summary>
		/// Construtor do filtro utilizado para logar excecoes e tratar o resultado adequadamente
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		public Exception4LogFilterAttribute(string loggerName)
		{
			Logger = LogManager.GetLogger(loggerName);
		}

		/// <summary>
		/// Construtor do filtro utilizado para logar excecoes e tratar o resultado adequadamente
		/// </summary>
		/// <param name="loggerName">Nome do Logger configurado no log4net</param>
		/// <param name="debugKey">Define uma chave de confianca para liberar acesso ao resultado com detalhes da excecao. Caso nao informado utilize o valor "true" para o cabecalho "X-DebugError"</param>
		/// <param name="headerName">Define um nome para o cabecalho responsavel por liberar acesso ao resultado com detalhes da excecao. Caso nao informado sera utilizado o nome "X-DebugError"</param>
		public Exception4LogFilterAttribute(string loggerName, string debugKey, string headerName = HEADER_DEBUG) : this(loggerName)
		{
			DebugKey = debugKey;
			HeaderName = headerName;
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

			Guid errorId = Guid.NewGuid();
			DateTime dtError = DateTime.Now;

			if (actionExecutedContext.Exception is ControllerException)
			{
				errorId = (actionExecutedContext.Exception as ControllerException).Id;
				dtError = (actionExecutedContext.Exception as ControllerException).DateTimeError;
			}

			var logObj = actionExecutedContext.Exception.ToDynamicObject(errorId);

			string jsonMsg = JsonConvert.SerializeObject(logObj);

			if (!actionExecutedContext.ActionContext.ActionDescriptor.IgnoreFilters()) Logger.Error(jsonMsg);

			bool returnErrorDetail = false;

			if (actionExecutedContext.Request.Headers.Contains(HeaderName))
			{
				var values = actionExecutedContext.Request.Headers.GetValues(HeaderName).Select(v => v.ToLower()).ToList();

				returnErrorDetail = (string.IsNullOrWhiteSpace(DebugKey) && values.Contains("true"))
					|| (!string.IsNullOrWhiteSpace(DebugKey) && values.Contains(DebugKey.ToLower()));
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
