using System;
using System.Web.Http;

namespace WebApiFilters4Log.WebApiTest.Controllers
{
	public class Action4LogController : ApiController
	{
		[HttpGet]
		[Action4LogFilter]
		public IHttpActionResult LogInfoWithHttpGet_Success()
		{
			return Ok(Models.ClientModel.GetFakeClient());
		}

		[HttpGet]
		[Action4LogFilter("Action4LogTest", LogLevel.INFO)]
		public IHttpActionResult LogInfoWithHttpGet_Fail()
		{
			throw new InvalidOperationException("LogInfoWithHttpGet_Fail");
		}

		[HttpPost]
		[Infra.FakeUserFilter("UserTest")]
		[Action4LogFilter("Action4LogTest", LogLevel.DEBUG, 2, MonitoredTypes = null, MessageStartingAction = "Inicio", MessageEndAction = "Fim")]
		public IHttpActionResult LogInfoWithHttpGet_WarnTimeout(Models.ClientModel client)
		{
			System.Threading.Thread.Sleep(3000);
			return Ok("Success");
		}

		[HttpPost]
		[Action4LogFilter("Action4LogTest", MonitoredTypes = "*", ArgumentsLogLevel = LogLevel.INFO, ArgumentsMessage = "argumentos")]
		public IHttpActionResult LogInfoWithHttpGet_OnlyFail(Models.ClientModel client)
		{
			throw new InvalidOperationException("LogInfoWithHttpGet_OnlyFail");
		}

		[HttpGet]
		[Action4LogFilter]
		[IgnoreLog]
		public IHttpActionResult LogInfoWithHttpGet_IgnoreFilters()
		{
			return Ok(Models.ClientModel.GetFakeClient());
		}

	}
}
