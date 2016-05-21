using System.Web.Http;

namespace WebApiFilters4Log.WebApiTest.Controllers
{
	public class Arguments4LogController : ApiController
	{
		[HttpGet]
		[Arguments4LogFilter("Args4Log")]
		public IHttpActionResult LogPrimitiveTypes(int id, decimal value, string text)
		{
			return Ok();
		}

		[HttpPost]
		[Arguments4LogFilter("Args4Log", "*", ArgumentsLogLevel = LogLevel.DEBUG)]
		public IHttpActionResult LogComplexTypes(Models.ClientModel client)
		{
			return Ok();
		}

		[HttpPut]
		[Arguments4LogFilter("Args4Log", "WebApiFilters4Log.WebApiTest.Models.ClientModel", ArgumentsLogLevel = LogLevel.WARN, ArgumentsMessage = "argumentos")]
		public IHttpActionResult LogInformedComplexTypes([FromUri] int id, [FromBody] Models.ClientModel client)
		{
			return Ok();
		}
	}
}
