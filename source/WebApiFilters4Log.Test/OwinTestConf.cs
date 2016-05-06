namespace WebApiFilters4Log.Test
{
	using Owin;
	using System.Web.Http;
	using WebApiTest.Infra;

	public class OwinTestConf
	{
		public void Configuration(IAppBuilder app)
		{
			HttpConfiguration config = new HttpConfiguration();

			log4net.Config.XmlConfigurator.Configure();

			WebApiConfig.Register(config);

			app.UseWebApi(config);
		}
	}
}
