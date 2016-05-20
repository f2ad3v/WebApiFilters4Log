namespace WebApiFilters4Log
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http.Controllers;

	/// <summary>
	/// Responsavel por capturar informacoes do HttpActionContext
	/// </summary>
	[Serializable]
	public class ControllerException : CommonException
	{
		const string MSG_ERROR_FORMAT = "Ocorreu um erro inesperado executando {0}.{1}!";
		const string CONTEXT_ID = "ContextId";

		/// <summary>
		/// Id do contexto da requisicao
		/// </summary>
		public Guid ContextId { get; private set; }

		/// <summary>
		/// Nome da maquina que gerou a excecao
		/// </summary>
		public string MachineName { get; private set; }

		/// <summary>
		/// Nome do controller que gerou a excecao
		/// </summary>
		public string Controller { get; private set; }

		/// <summary>
		/// Nome da action que gerou a excecao
		/// </summary>
		public string Action { get; private set; }

		/// <summary>
		/// URL e parametros da requisicao
		/// </summary>
		public string PathAndQuery { get; private set; }

		/// <summary>
		/// Usuario que realizou a requisicao
		/// </summary>
		public string User { get; private set; }

		/// <summary>
		/// Argumentos passados na requisicao
		/// </summary>
		public Dictionary<string, string> Arguments { get; private set; }

		/// <summary>
		/// Construtor utilizado para capturar informacoes relevantes do HttpActionContext
		/// </summary>
		/// <param name="actionContext">Instancia de HttpActionContext corrente</param>
		/// <param name="innerException">Excecao capturada</param>
		public ControllerException(HttpActionContext actionContext, Exception innerException)
			: base(GetMessageError(actionContext.ActionDescriptor), Guid.NewGuid(), innerException)
		{
			Guid contextId = ContextId = Guid.Empty;

			var contextIdStr = string.Empty;
			if (actionContext.Request.Headers.Contains(CONTEXT_ID))
				contextIdStr = actionContext.Request.Headers.GetValues(CONTEXT_ID).SingleOrDefault();

			if (!string.IsNullOrWhiteSpace(contextIdStr) && Guid.TryParse(contextIdStr, out contextId))
				ContextId = contextId;

			Controller = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
			Action = actionContext.ActionDescriptor.ActionName;

			MachineName = Environment.MachineName;

			PathAndQuery = actionContext.Request.RequestUri.PathAndQuery;

			User = "Anonymous";

			if (actionContext.RequestContext.Principal != null
				&& actionContext.RequestContext.Principal.Identity != null
				&& !string.IsNullOrWhiteSpace(actionContext.RequestContext.Principal.Identity.Name))
				User = actionContext.RequestContext.Principal.Identity.Name;

			Arguments = actionContext.ActionArguments.GetActionArguments("*");
		}

		private static string GetMessageError(HttpActionDescriptor actionDescriptor)
		{
			return string.Format(MSG_ERROR_FORMAT, actionDescriptor.ControllerDescriptor.ControllerName, actionDescriptor.ActionName);
		}
	}
}
