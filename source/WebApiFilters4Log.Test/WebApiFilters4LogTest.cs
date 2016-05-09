using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using log4net;

namespace WebApiFilters4Log.Test
{
	[TestClass]
	public class WebApiFilters4LogTest
	{
		private const string LOG_FILE_NAME = ".\\log\\{0}.log";
		private const string LOG_FILE_NAME_TMP = ".\\log\\{0}Tmp.log";
		private static string jsonModelClient = Newtonsoft.Json.JsonConvert.SerializeObject(WebApiTest.Models.ClientModel.GetFakeClient()).Replace("\"", "'");


		[TestMethod]
		public async Task TestFilters()
		{
			var action4LogFileName = string.Format(LOG_FILE_NAME, "Action4Log");
			var action4LogFileNameTmp = string.Format(LOG_FILE_NAME_TMP, "Action4Log");
			var args4LogFileName = string.Format(LOG_FILE_NAME, "Args4Log");
			var args4LogFileNameTmp = string.Format(LOG_FILE_NAME_TMP, "Args4Log");
			var exception4LogFileName = string.Format(LOG_FILE_NAME, "Exceptions4Log");
			var exception4LogFileNameTmp = string.Format(LOG_FILE_NAME_TMP, "Exceptions4Log");
			var extension4LogFileName = string.Format(LOG_FILE_NAME, "Extension4Log");
			var extension4LogFileNameTmp = string.Format(LOG_FILE_NAME_TMP, "Extension4Log");

			if (File.Exists(action4LogFileName)) File.Delete(action4LogFileName);
			if (File.Exists(action4LogFileNameTmp)) File.Delete(action4LogFileNameTmp);
			if (File.Exists(args4LogFileName)) File.Delete(args4LogFileName);
			if (File.Exists(args4LogFileNameTmp)) File.Delete(args4LogFileNameTmp);
			if (File.Exists(exception4LogFileName)) File.Delete(exception4LogFileName);
			if (File.Exists(exception4LogFileNameTmp)) File.Delete(exception4LogFileNameTmp);
			if (File.Exists(extension4LogFileName)) File.Delete(extension4LogFileName);
			if (File.Exists(extension4LogFileNameTmp)) File.Delete(extension4LogFileNameTmp);

			using (var server = TestServer.Create<OwinTestConf>())
			using (var client = new HttpClient(server.Handler))
			{
				var response = await client.GetAsync("http://testserver/api/Action4Log/LogInfoWithHttpGet_Success");

				Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

				var result = await response.Content.ReadAsAsync<string>();

				Assert.AreEqual("Success", result);

				response = await client.GetAsync("http://testserver/api/Action4Log/LogInfoWithHttpGet_Fail");

				Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

				response = await client.GetAsync("http://testserver/api/Arguments4Log/LogPrimitiveTypes?id=6&value=2.34&text=testing");

				Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				response = await client.PostAsJsonAsync("http://testserver/api/Arguments4Log/LogComplexTypes", WebApiTest.Models.ClientModel.GetFakeClient());

				Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

				response = await client.PutAsJsonAsync("http://testserver/api/Arguments4Log/LogInformedComplexTypes?id=8", WebApiTest.Models.ClientModel.GetFakeClient());

				Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

				response = await client.GetAsync("http://testserver/api/Exception4Log/LogSimpleException");

				Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

				result = await response.Content.ReadAsStringAsync();

				var dicResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

				Assert.IsTrue(dicResult.ContainsKey("Message"));
				Assert.AreEqual("testando log...", dicResult["Message"]);

				Assert.IsTrue(dicResult.ContainsKey("ErrorCode"));
				Assert.AreEqual("500", dicResult["ErrorCode"]);

				Assert.IsTrue(dicResult.ContainsKey("ErrorId"));
				Assert.IsTrue(dicResult.ContainsKey("DateTime"));

				client.DefaultRequestHeaders.Add("X-DebugError", "true");

				response = await client.GetAsync("http://testserver/api/Exception4Log/LogSimpleException");

				Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

				result = await response.Content.ReadAsStringAsync();

				TestSimpleExceptionInfo(result);

				client.DefaultRequestHeaders.Remove("X-DebugError");
				client.DefaultRequestHeaders.Add("DebugError", "true");

				response = await client.PostAsJsonAsync("http://testserver/api/Exception4Log/LogDetailedException", WebApiTest.Models.ClientModel.GetFakeClient());

				Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

				result = await response.Content.ReadAsStringAsync();

				TestDetailedExceptionInfo(result, "LogDetailedException", false);

				client.DefaultRequestHeaders.Remove("DebugError");
				client.DefaultRequestHeaders.Add("X-DebugError", "0d059126-2ccb-40db-b65b-c020dd5b0810");

				response = await client.PutAsJsonAsync("http://testserver/api/Exception4Log/LogDetailedExceptionWithDebugKey?id=eb60d56e-5b4a-4f13-a9f2-925b9297c9c9", WebApiTest.Models.ClientModel.GetFakeClient());

				Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

				result = await response.Content.ReadAsStringAsync();

				TestDetailedExceptionInfo(result, "LogDetailedExceptionWithDebugKey", true);
			}

			TestExtension4Log(extension4LogFileName, extension4LogFileNameTmp);

			TestAction4Log(action4LogFileName, action4LogFileNameTmp);

			TestArguments4Log(args4LogFileName, args4LogFileNameTmp);

			TestException4Log(exception4LogFileName, exception4LogFileNameTmp);
		}

		private void TestExtension4Log(string extension4LogFileName, string extension4LogFileNameTmp)
		{
			ILog logger = LogManager.GetLogger("Extension4Log");

			Assert.IsTrue(logger.IsEnabled(LogLevel.DEBUG));
			Assert.IsTrue(logger.IsEnabled(LogLevel.INFO));
			Assert.IsTrue(logger.IsEnabled(LogLevel.WARN));
			Assert.IsTrue(logger.IsEnabled(LogLevel.ERROR));
			Assert.IsTrue(logger.IsEnabled(LogLevel.FATAL));

			logger.LogMessage(LogLevel.DEBUG, "Test DEBUG");
			logger.LogMessage(LogLevel.DEBUG, "{0} {1}", "Test DEBUG", "[format]");

			logger.LogMessage(LogLevel.INFO, "Test INFO");
			logger.LogMessage(LogLevel.INFO, "{0} {1}", "Test INFO", "[format]");

			logger.LogMessage(LogLevel.WARN, "Test WARN");
			logger.LogMessage(LogLevel.WARN, "{0} {1}", "Test WARN", "[format]");

			logger.LogMessage(LogLevel.ERROR, "Test ERROR");
			logger.LogMessage(LogLevel.ERROR, "{0} {1}", "Test ERROR", "[format]");

			logger.LogMessage(LogLevel.FATAL, "Test FATAL");
			logger.LogMessage(LogLevel.FATAL, "{0} {1}", "Test FATAL", "[format]");

			Dictionary<string, string> dic = new Dictionary<string, string> { { "test", "ok" } };
			logger.LogMessage(LogLevel.DEBUG, dic, "Test DEBUG");
			logger.LogMessage(LogLevel.DEBUG, dic, "{0} {1}", "Test DEBUG", "[format]");

			Assert.IsTrue(File.Exists(extension4LogFileName));

			File.Copy(extension4LogFileName, extension4LogFileNameTmp);

			var lines = File.ReadAllLines(extension4LogFileNameTmp);

			Assert.AreEqual(12, lines.Length);

			var log = new LogInfo(lines[0]);

			Assert.AreEqual(LogLevel.DEBUG.ToString(), log.LogLevel);
			Assert.AreEqual("Test DEBUG", log.Message);

			log = new LogInfo(lines[1]);

			Assert.AreEqual(LogLevel.DEBUG.ToString(), log.LogLevel);
			Assert.AreEqual("Test DEBUG [format]", log.Message);

			log = new LogInfo(lines[2]);

			Assert.AreEqual(LogLevel.INFO.ToString(), log.LogLevel);
			Assert.AreEqual("Test INFO", log.Message);

			log = new LogInfo(lines[3]);

			Assert.AreEqual(LogLevel.INFO.ToString(), log.LogLevel);
			Assert.AreEqual("Test INFO [format]", log.Message);

			log = new LogInfo(lines[4]);

			Assert.AreEqual(LogLevel.WARN.ToString(), log.LogLevel);
			Assert.AreEqual("Test WARN", log.Message);

			log = new LogInfo(lines[5]);

			Assert.AreEqual(LogLevel.WARN.ToString(), log.LogLevel);
			Assert.AreEqual("Test WARN [format]", log.Message);

			log = new LogInfo(lines[6]);

			Assert.AreEqual(LogLevel.ERROR.ToString(), log.LogLevel);
			Assert.AreEqual("Test ERROR", log.Message);

			log = new LogInfo(lines[7]);

			Assert.AreEqual(LogLevel.ERROR.ToString(), log.LogLevel);
			Assert.AreEqual("Test ERROR [format]", log.Message);

			log = new LogInfo(lines[8]);

			Assert.AreEqual(LogLevel.FATAL.ToString(), log.LogLevel);
			Assert.AreEqual("Test FATAL", log.Message);

			log = new LogInfo(lines[9]);

			Assert.AreEqual(LogLevel.FATAL.ToString(), log.LogLevel);
			Assert.AreEqual("Test FATAL [format]", log.Message);

			log = new LogInfo(lines[10]);

			Assert.AreEqual(LogLevel.DEBUG.ToString(), log.LogLevel);
			Assert.AreEqual("Test DEBUG", log.Message);
			Assert.IsTrue(log.Context.ContainsKey("test"));
			Assert.AreEqual("ok", log.Context["test"]);

			log = new LogInfo(lines[11]);

			Assert.AreEqual(LogLevel.DEBUG.ToString(), log.LogLevel);
			Assert.AreEqual("Test DEBUG [format]", log.Message);
			Assert.IsTrue(log.Context.ContainsKey("test"));
			Assert.AreEqual("ok", log.Context["test"]);
		}

		#region TestException4Log

		private void TestException4Log(string exception4LogFileName, string exception4LogFileNameTmp)
		{
			Assert.IsTrue(File.Exists(exception4LogFileName));

			File.Copy(exception4LogFileName, exception4LogFileNameTmp);

			var lines = File.ReadAllLines(exception4LogFileNameTmp);

			Assert.AreEqual(4, lines.Length);

			TestSimpleExceptionInfo(lines[0]);
			TestSimpleExceptionInfo(lines[1]);
			TestDetailedExceptionInfo(lines[2], "LogDetailedException", false);
			TestDetailedExceptionInfo(lines[3], "LogDetailedExceptionWithDebugKey", true);
		}
		private static void TestSimpleExceptionInfo(string exceptionResult)
		{
			var exInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ExceptionInfo>(exceptionResult);

			Assert.AreEqual("testando log...", exInfo.Message);
			Assert.AreEqual("WebApiFilters4Log.WebApiTest", exInfo.Source);
			Assert.AreEqual("System.InvalidOperationException", exInfo.ExceptionType);
			Assert.IsTrue(exInfo.StackTrace.Contains("ThrowExceptionLevel1"));
			Assert.IsNull(exInfo.InnerException);
		}

		private static void TestDetailedExceptionInfo(string exceptionResult, string actionName, bool testPut)
		{
			var exInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ExceptionInfo>(exceptionResult);

			Assert.AreEqual(string.Format("Ocorreu um erro inesperado executando Exception4Log.{0}!", actionName), exInfo.Message);
			Assert.AreEqual("WebApiFilters4Log.WebApiTest", exInfo.Source);
			Assert.AreEqual("WebApiFilters4Log.ControllerException", exInfo.ExceptionType);
			Assert.IsTrue(exInfo.StackTrace.Contains(actionName));

			Assert.IsTrue(exInfo.AdditionalInfo.ContainsKey("Controller"));
			Assert.AreEqual("Exception4Log", exInfo.AdditionalInfo["Controller"]);
			Assert.IsTrue(exInfo.AdditionalInfo.ContainsKey("Action"));
			Assert.AreEqual(actionName, exInfo.AdditionalInfo["Action"]);
			Assert.IsTrue(exInfo.AdditionalInfo.ContainsKey("PathAndQuery"));

			string pathAndQuery = string.Format("/api/Exception4Log/{0}", actionName);
			if (testPut) pathAndQuery = string.Format("/api/Exception4Log/{0}?id=eb60d56e-5b4a-4f13-a9f2-925b9297c9c9", actionName);

			Assert.AreEqual(pathAndQuery, exInfo.AdditionalInfo["PathAndQuery"]);

			if (testPut)
			{
				Assert.IsTrue(exInfo.AdditionalInfo.ContainsKey("System.Guid id"));
				Assert.AreEqual("'eb60d56e-5b4a-4f13-a9f2-925b9297c9c9'", exInfo.AdditionalInfo["System.Guid id"]);
			}

			Assert.IsTrue(exInfo.AdditionalInfo.ContainsKey("WebApiFilters4Log.WebApiTest.Models.ClientModel client"));
			Assert.AreEqual(jsonModelClient, exInfo.AdditionalInfo["WebApiFilters4Log.WebApiTest.Models.ClientModel client"]);

			Assert.IsNotNull(exInfo.InnerException);

			Assert.AreEqual("testando log...", exInfo.InnerException.Message);
			Assert.AreEqual("WebApiFilters4Log.WebApiTest", exInfo.InnerException.Source);
			Assert.AreEqual("System.InvalidOperationException", exInfo.InnerException.ExceptionType);
			Assert.IsTrue(exInfo.InnerException.StackTrace.Contains("ThrowExceptionLevel1"));
			Assert.IsNull(exInfo.InnerException.InnerException);
		}

		#endregion TestException4Log

		#region TestAction4Log

		private void TestAction4Log(string action4LogFileName, string action4LogFileNameTmp)
		{
			Assert.IsTrue(File.Exists(action4LogFileName));

			File.Copy(action4LogFileName, action4LogFileNameTmp);

			var lines = File.ReadAllLines(action4LogFileNameTmp);

			Assert.AreEqual(4, lines.Length);

			TestActionLogHttpGetSuccess(lines[0], lines[1]);
			TestActionLogHttpGetFail(lines[2], lines[3]);
		}

		private static void TestActionLogHttpGetSuccess(string strLogStart, string strLogEnd)
		{
			var logStart = new LogInfo(strLogStart);

			Assert.IsTrue(logStart.DateTimeLog.HasValue);
			Assert.AreEqual(DateTime.Now.ToString("yyyyMMddHH"), logStart.DateTimeLog.Value.ToString("yyyyMMddHH"));

			Assert.AreEqual("Starting Action", logStart.Message);
			Assert.AreEqual("DEBUG", logStart.LogLevel);

			Assert.IsTrue(logStart.Context.ContainsKey("ContextId"));

			Guid contextId;
			if (!Guid.TryParse(logStart.Context["ContextId"], out contextId))
				Assert.Fail("ContextId nao e do tipo Guid");

			Assert.IsTrue(logStart.Context.ContainsKey("MachineName"));
			Assert.AreEqual(Environment.MachineName, logStart.Context["MachineName"]);

			Assert.IsTrue(logStart.Context.ContainsKey("Controller"));
			Assert.AreEqual("Action4Log", logStart.Context["Controller"]);

			Assert.IsTrue(logStart.Context.ContainsKey("Action"));
			Assert.AreEqual("LogInfoWithHttpGet_Success", logStart.Context["Action"]);

			Assert.IsTrue(logStart.Context.ContainsKey("Method"));
			Assert.AreEqual("GET", logStart.Context["Method"]);

			var logEnd = new LogInfo(strLogEnd);

			Assert.AreEqual("End Action", logEnd.Message);
			Assert.AreEqual("DEBUG", logEnd.LogLevel);

			Assert.IsTrue(logEnd.Context.ContainsKey("ContextId"));

			Guid contextIdEnd;
			if (!Guid.TryParse(logEnd.Context["ContextId"], out contextIdEnd))
				Assert.Fail("ContextId nao e do tipo Guid");

			Assert.AreEqual(contextId, contextIdEnd);

			Assert.IsTrue(logEnd.Context.ContainsKey("Time"));

			Assert.IsTrue(logEnd.Context.ContainsKey("StatusCode"));
			Assert.AreEqual("OK(200)", logEnd.Context["StatusCode"]);
		}

		private static void TestActionLogHttpGetFail(string strLogStart, string strLogEnd)
		{
			var logStart = new LogInfo(strLogStart);

			Assert.IsTrue(logStart.DateTimeLog.HasValue);
			Assert.AreEqual(DateTime.Now.ToString("yyyyMMddHH"), logStart.DateTimeLog.Value.ToString("yyyyMMddHH"));

			Assert.AreEqual("Starting Action", logStart.Message);
			Assert.AreEqual("INFO", logStart.LogLevel);

			Assert.IsTrue(logStart.Context.ContainsKey("ContextId"));

			Guid contextId;
			if (!Guid.TryParse(logStart.Context["ContextId"], out contextId))
				Assert.Fail("ContextId nao e do tipo Guid");

			var logEnd = new LogInfo(strLogEnd);

			Assert.AreEqual("End Action", logEnd.Message);
			Assert.AreEqual("ERROR", logEnd.LogLevel);

			Assert.IsTrue(logEnd.Context.ContainsKey("ContextId"));

			Guid contextIdEnd;
			if (!Guid.TryParse(logEnd.Context["ContextId"], out contextIdEnd))
				Assert.Fail("ContextId nao e do tipo Guid");

			Assert.AreEqual(contextId, contextIdEnd);
		}

		#endregion TestAction4Log

		#region TestArguments4Log

		private void TestArguments4Log(string args4LogFileName, string args4LogFileNameTmp)
		{
			Assert.IsTrue(File.Exists(args4LogFileName));

			File.Copy(args4LogFileName, args4LogFileNameTmp);

			var lines = File.ReadAllLines(args4LogFileNameTmp);

			Assert.AreEqual(3, lines.Length);

			TestArg4LogPrimitiveTypes(lines[0]);
			TestArg4LogComplexTypes(lines[1]);
			TestArg4LogInformedComplexTypes(lines[2]);
		}

		private static void TestArg4LogPrimitiveTypes(string line)
		{
			var logArgInfo = new LogArgsInfo(line);

			Assert.IsTrue(logArgInfo.DateTimeLog.HasValue);
			Assert.AreEqual(DateTime.Now.ToString("yyyyMMddHH"), logArgInfo.DateTimeLog.Value.ToString("yyyyMMddHH"));

			Assert.AreEqual("DEBUG", logArgInfo.LogLevel);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("MachineName"));
			Assert.AreEqual(Environment.MachineName, logArgInfo.Context["MachineName"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Controller"));
			Assert.AreEqual("Arguments4Log", logArgInfo.Context["Controller"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Action"));
			Assert.AreEqual("LogPrimitiveTypes", logArgInfo.Context["Action"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Method"));
			Assert.AreEqual("GET", logArgInfo.Context["Method"]);

			Assert.AreEqual(3, logArgInfo.Arguments.Count);

			Assert.IsTrue(logArgInfo.Arguments.ContainsKey("System.Int32 id"));
			Assert.AreEqual("6", logArgInfo.Arguments["System.Int32 id"]);

			Assert.IsTrue(logArgInfo.Arguments.ContainsKey("System.Decimal value"));
			Assert.AreEqual("2.34", logArgInfo.Arguments["System.Decimal value"]);

			Assert.IsTrue(logArgInfo.Arguments.ContainsKey("System.String text"));
			Assert.AreEqual("'testing'", logArgInfo.Arguments["System.String text"]);
		}

		private static void TestArg4LogComplexTypes(string line)
		{
			var logArgInfo = new LogArgsInfo(line);

			Assert.IsTrue(logArgInfo.DateTimeLog.HasValue);
			Assert.AreEqual(DateTime.Now.ToString("yyyyMMddHH"), logArgInfo.DateTimeLog.Value.ToString("yyyyMMddHH"));

			Assert.AreEqual("INFO", logArgInfo.LogLevel);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("MachineName"));
			Assert.AreEqual(Environment.MachineName, logArgInfo.Context["MachineName"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Controller"));
			Assert.AreEqual("Arguments4Log", logArgInfo.Context["Controller"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Action"));
			Assert.AreEqual("LogComplexTypes", logArgInfo.Context["Action"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Method"));
			Assert.AreEqual("POST", logArgInfo.Context["Method"]);

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(WebApiTest.Models.ClientModel.GetFakeClient()).Replace("\"", "'");

			Assert.IsTrue(logArgInfo.Arguments.ContainsKey("WebApiFilters4Log.WebApiTest.Models.ClientModel client"));
			Assert.AreEqual(json, logArgInfo.Arguments["WebApiFilters4Log.WebApiTest.Models.ClientModel client"]);
		}

		private static void TestArg4LogInformedComplexTypes(string line)
		{
			var logArgInfo = new LogArgsInfo(line);

			Assert.IsTrue(logArgInfo.DateTimeLog.HasValue);
			Assert.AreEqual(DateTime.Now.ToString("yyyyMMddHH"), logArgInfo.DateTimeLog.Value.ToString("yyyyMMddHH"));

			Assert.AreEqual("WARN", logArgInfo.LogLevel);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("MachineName"));
			Assert.AreEqual(Environment.MachineName, logArgInfo.Context["MachineName"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Controller"));
			Assert.AreEqual("Arguments4Log", logArgInfo.Context["Controller"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Action"));
			Assert.AreEqual("LogInformedComplexTypes", logArgInfo.Context["Action"]);

			Assert.IsTrue(logArgInfo.Context.ContainsKey("Method"));
			Assert.AreEqual("PUT", logArgInfo.Context["Method"]);

			Assert.AreEqual(1, logArgInfo.Arguments.Count);

			Assert.IsTrue(logArgInfo.Arguments.ContainsKey("WebApiFilters4Log.WebApiTest.Models.ClientModel client"));
			Assert.AreEqual(jsonModelClient, logArgInfo.Arguments["WebApiFilters4Log.WebApiTest.Models.ClientModel client"]);
		}

		#endregion TestArguments4Log
	}
}
