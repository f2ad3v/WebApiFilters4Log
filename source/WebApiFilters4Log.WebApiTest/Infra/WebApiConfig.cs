namespace WebApiFilters4Log.WebApiTest.Infra
{
	using System.Web.Http;

	public class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				name: "ControllersApi",
				 routeTemplate: "api/{controller}/{action}/{id}",
					defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
