namespace WebApiFilters4Log
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http.Controllers;

	/// <summary>
	/// Responsavel por capturar informacoes do HttpActionContext
	/// </summary>
	public class ControllerException : CommonException
	{
		private const string MSG_ERROR = "Ocorreu um erro inesperado executando ";
		const string CONTEXT_ID = "ContextId";

		/// <summary>
		/// Id do contexto da requisicao
		/// </summary>
		public Guid ContextId { get; set; }

		/// <summary>
		/// Nome da maquina que gerou a excecao
		/// </summary>
		public string MachineName { get; set; }

		/// <summary>
		/// Nome do controller que gerou a excecao
		/// </summary>
		public string Controller { get; set; }

		/// <summary>
		/// Nome da action que gerou a excecao
		/// </summary>
		public string Action { get; set; }

		/// <summary>
		/// URL e parametros da requisicao
		/// </summary>
		public string PathAndQuery { get; set; }

		/// <summary>
		/// Usuario que realizou a requisicao
		/// </summary>
		public string User { get; set; }

		/// <summary>
		/// Argumentos passados na requisicao
		/// </summary>
		public Dictionary<string, string> Arguments { get; set; }

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

			if (actionContext.RequestContext.Principal != null
				&& actionContext.RequestContext.Principal.Identity != null
				&& !string.IsNullOrWhiteSpace(actionContext.RequestContext.Principal.Identity.Name))
				User = actionContext.RequestContext.Principal.Identity.Name;

			Arguments = actionContext.ActionArguments.GetActionArguments("*");
		}

		private static string GetMessageError(HttpActionDescriptor actionDescriptor)
		{
			return string.Format("{0}{1}.{2}!", MSG_ERROR, actionDescriptor.ControllerDescriptor.ControllerName, actionDescriptor.ActionName);
		}
	}
}
