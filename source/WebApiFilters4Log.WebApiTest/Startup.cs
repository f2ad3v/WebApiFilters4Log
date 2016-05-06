[assembly: Microsoft.Owin.OwinStartup(typeof(WebApiFilters4Log.WebApiTest.Startup))]
namespace WebApiFilters4Log.WebApiTest
{
	using Infra;
	using Owin;
	using System.Web.Http;

	public class Startup
	{
		public void Configuration(IAppBuilder appBuilder)
		{
			log4net.Config.XmlConfigurator.Configure();

			HttpConfiguration httpConfiguration = new HttpConfiguration();

			WebApiConfig.Register(httpConfiguration);

			appBuilder.UseWebApi(httpConfiguration);
		}
	}
}
