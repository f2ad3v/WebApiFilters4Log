using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebApiFilters4Log.WebApiTest.Infra
{
	public class FakeUserFilterAttribute : ActionFilterAttribute
	{
		private string UserName = string.Empty;

		public FakeUserFilterAttribute(string userName)
		{
			UserName = userName;
		}

		public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			actionContext.RequestContext.Principal = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(UserName), new string[] { "Adm" });
			return base.OnActionExecutingAsync(actionContext, cancellationToken);
		}
	}
}