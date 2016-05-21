using System;
using System.Web.Http;

namespace WebApiFilters4Log.WebApiTest.Controllers
{
	public class Exception4LogController : ApiController
	{
		[HttpGet]
		[Exception4LogFilter("ExceptionLogger")]
		public IHttpActionResult LogSimpleException()
		{
			ThrowExceptionLevel1();
			return Ok(Models.ClientModel.GetFakeClient());
		}

		[HttpPost]
		[Infra.FakeUserFilter("UserTest")]
		[Exception4LogFilter("ExceptionLogger", HeaderName = "DebugError", DebugKey = "")]
		public IHttpActionResult LogDetailedException(Models.ClientModel client)
		{
			try
			{
				ThrowExceptionLevel1();
				return Ok(client);
			}
			catch (Exception ex)
			{
				throw new ControllerException(ActionContext, ex);
			}
		}

		[HttpPut]
		[Action4LogFilter("ERRORExceptionLogger", LogLevel.DEBUG)]
		[Exception4LogFilter("ExceptionLogger", "0d059126-2ccb-40db-b65b-c020dd5b0810")]
		public IHttpActionResult LogDetailedExceptionWithDebugKey([FromUri] Guid id, [FromBody] Models.ClientModel client)
		{
			try
			{
				ThrowExceptionLevel1();
				return Ok(client);
			}
			catch (Exception ex)
			{
				throw new ControllerException(ActionContext, ex);
			}
		}

		private void ThrowExceptionLevel1()
		{
			ThrowExceptionLevel2();
		}

		private void ThrowExceptionLevel2()
		{
			throw new InvalidOperationException("testando log...");
		}
	}
}
