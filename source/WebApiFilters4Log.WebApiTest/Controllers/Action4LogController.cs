using System;
using System.Web.Http;

namespace WebApiFilters4Log.WebApiTest.Controllers
{
	public class Action4LogController : ApiController
	{
		[HttpGet]
		[Action4LogFilter()]
		public IHttpActionResult LogInfoWithHttpGet_Success()
		{
			return Ok("Success");
		}

		[HttpGet]
		[Action4LogFilter("Action4LogTest", LogLevel.INFO)]
		public IHttpActionResult LogInfoWithHttpGet_Fail()
		{
			throw new InvalidOperationException("LogInfoWithHttpGet_Fail");
		}

		[HttpPost]
		[Infra.FakeUserFilter("UserTest")]
		[Action4LogFilter("Action4LogTest", LogLevel.DEBUG, TimeOutWarn = 2, MessageStartingAction = "Inicio", MessageEndAction = "Fim")]
		[Arguments4LogFilter("TESTArgs4Log", LogLevel.INFO, null)]
		public IHttpActionResult LogInfoWithHttpGet_WarnTimeout(Models.ClientModel client)
		{
			System.Threading.Thread.Sleep(3000);
			return Ok("Success");
		}

		[HttpPost]
		[Action4LogFilter("ERRORAction4LogTest")]
		[Arguments4LogFilter("TESTArgs4Log", LogLevel.INFO, "*")]
		public IHttpActionResult LogInfoWithHttpGet_OnlyFail(Models.ClientModel client)
		{
			throw new InvalidOperationException("LogInfoWithHttpGet_OnlyFail");
		}
	}
}
