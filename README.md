# **WebApiFilters4Log** #

## O que é?

Biblioteca de filtros destinada a simplificar o monitoramento de uma aplicação OWIN ASP.NET Web API utilizando o log4net.

### Filtros:

#### Action4LogFilterAttribute
* Filtro derivado de [*System.Web.Http.Filters.ActionFilterAttribute*](https://msdn.microsoft.com/en-us/library/system.web.http.filters.actionfilterattribute(v=vs.118).aspx) destinado a registrar em *log* o início e o fim da execução de uma ação.

#### Arguments4LogFilterAttribute
* Filtro derivado de [*System.Web.Http.Filters.ActionFilterAttribute*](https://msdn.microsoft.com/en-us/library/system.web.http.filters.actionfilterattribute(v=vs.118).aspx) destinado a registrar em *log* os argumentos passados para uma ação.

#### Exception4LogFilterAttribute
* Filtro derivado de [*System.Web.Http.Filters.ExceptionFilterAttribute*](https://msdn.microsoft.com/en-us/library/system.web.http.filters.exceptionfilterattribute(v=vs.118).aspx) destinado a registrar em *log* e tratar o resultado adequadamente das exceções lançadas em uma ação.

### Exceções customizadas:

#### CommonException
* Exceção derivada de [*System.Exception*](https://msdn.microsoft.com/en-us/library/system.exception(v=vs.110).aspx) destinada a fornecer um identificador único para a exceção capturada. Também armazena a data de a hora em que ocorreu a exceção.

#### ControllerException
* Exceção derivada de *WebApiFilters4Log.CommonException* destinada a obter informações sobre o contexto de uma ação através de seu construtor.

### Métodos estendidos:

#### System.Web.Http.Controllers.HttpActionContext.**GetLogContext**
* Obtem um dicionário do tipo *Dictionary<string, string>* contendo detalhes da execução de uma ação. Detalhes retornados: identificador único do contexto; nome do servidor; nome do controlador; nome da ação; método HTTP executado na requisição; nome do usuário.

#### log4net.ILog.**LogMessage**
* Registra uma mensagem em log de acordo com o enumerador **WebApiFilters4Log.LogLevel** informado. Além de possibilitar registrar uma mensagem contendo um dicionário com os detalhes do contexto da ação.

---
## Como usar?

### Instalação:
Localize e instale o pacote *WebApiFilters4Log* através do NuGet Package Manager ou utilize o comando **Install-Package WebApiFilters4Log** no Package Manager Console.

### Utilização:
Após a instalação do *WebApiFilters4Log* via NuGet não é necessário realizar nenhum tipo de configuração. **Automagicamente** o arquivo de configuração e o arquivo *Startup.cs* são alterados para que o *WebApiFilters4Log* funcione corretamente.

#### Alterações realizadas no arquivo de configuração *Web.config*:
As alterações realizadas no arquivo de configuração são pertinentes ao log4net no qual são criados dois *Loggers*. O primeiro, nomeado como *Logger*, é responsável por registrar todos *logs*. O segundo, nomeado como *ExceptionLogger* é responsável por registrar os detalhes das exceções capturadas.

#### Alterações realizadas no arquivo *Startup.cs*:
As alterações realizadas no arquivo *Startup.cs* servem para iniciar o log4net com as configurações do web.config e também para registrar os filtros de forma global.

---
## Exemplos

### Aplicando os filtros globalmente

```csharp
public class Startup
{
	public void Configuration(IAppBuilder app)
	{
		// Configurando o log4net (NÃO REMOVER)
		// *O código abaixo é acrescentado automaticamente na instalação do WebApiFilters4Log
		log4net.Config.XmlConfigurator.Configure();

		var httpConfiguration = new HttpConfiguration();

		// O início e o fim de todas ações serão registradas como DEBUG no Logger.
		// WARN caso a duração ultrapasse 60 segundos e ERROR em caso de exceção.
		// *O código abaixo é acrescentado automaticamente na instalação do WebApiFilters4Log
		httpConfiguration.Filters.Add(new WebApiFilters4Log.Action4LogFilterAttribute("Logger", WebApiFilters4Log.LogLevel.DEBUG, 60));
		
		// Todas as exceções não tratadas serão registradas no ExceptionLogger e o resultado será tratado e retornado adequadamente
		// *O código abaixo é acrescentado automaticamente na instalação do WebApiFilters4Log
		httpConfiguration.Filters.Add(new WebApiFilters4Log.Exception4LogFilterAttribute("ExceptionLogger"));

		// Todos os argumentos enviados para ação serão serializados em json e registrados como INFO no Logger  
		httpConfiguration.Filters.Add(new WebApiFilters4Log.Arguments4LogFilterAttribute("Logger", WebApiFilters4Log.LogLevel.INFO, "*"));

		// ...
	}
}
```

### **Action4LogFilter** - Aplicando a um controlador

```csharp
[Action4LogFilter("Logger")]
public class Action4LogController : ApiController
{
	[HttpGet]
	public IHttpActionResult LogInfoWithHttpGet_Success()
	{
		return Ok("Success");
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:50:43.250] [7] DEBUG - {"ContextId":"ba2dd75c-f35f-44c8-a3ef-b79a009f39c3","MachineName":"F2ASRV","Controller":"Action4Log","Action":"LogInfoWithHttpGet_Success","Method":"GET","UserName":"Anonymous"} - Starting Action
	// [20160505 19:50:43.311] [7] DEBUG - {"ContextId":"ba2dd75c-f35f-44c8-a3ef-b79a009f39c3","Time":"0,0705","StatusCode":"OK(200)"} - End Action

	[HttpGet]
	public IHttpActionResult LogInfoWithHttpGet_Fail()
	{
		throw new InvalidOperationException("LogInfoWithHttpGet_Fail");
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:50:43.379] [10] DEBUG - {"ContextId":"4e067583-41d2-47c8-beae-9b191fe8a7cb","MachineName":"F2ASRV","Controller":"Action4Log","Action":"LogInfoWithHttpGet_Fail","Method":"GET","UserName":"Anonymous"} - Starting Action
	// [20160505 19:50:43.380] [10] ERROR - {"ContextId":"4e067583-41d2-47c8-beae-9b191fe8a7cb","Time":"0,0010"} - End Action
}
```

### **Action4LogFilter** - Aplicando a uma ação

```csharp
public class Action4LogController : ApiController
{
	[HttpGet]
	[Action4LogFilter("Logger", LogLevel.INFO)]
	public IHttpActionResult LogInfoWithHttpGet_Success()
	{
		return Ok("Success");
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:50:40.031] [7] INFO - {"ContextId":"3430fb9c-20cf-4af8-baf4-5367c36a85fb","MachineName":"F2ASRV","Controller":"Action4Log","Action":"LogInfoWithHttpGet_Success","Method":"GET","UserName":"Anonymous"} - Starting Action
	// [20160505 19:50:40.088] [7] INFO - {"ContextId":"3430fb9c-20cf-4af8-baf4-5367c36a85fb","Time":"0,0648","StatusCode":"OK(200)"} - End Action

	[HttpGet]
	[Action4LogFilter("Logger", LogLevel.WARN)]
	public IHttpActionResult LogInfoWithHttpGet_Fail()
	{
		throw new InvalidOperationException("LogInfoWithHttpGet_Fail");
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:50:43.379] [10] WARN - {"ContextId":"898a89d7-d01-4956-8c84-15f659460b34","MachineName":"F2ASRV","Controller":"Action4Log","Action":"LogInfoWithHttpGet_Fail","Method":"GET","UserName":"Anonymous"} - Starting Action
	// [20160505 19:50:43.380] [10] ERROR - {"ContextId":"898a89d7-dd01-4956-8c84-15f659460b34","Time":"0,0010"} - End Action

	[HttpGet]
	[Action4LogFilter("Logger", LogLevel.DEBUG, TimeOutWarn = 30, MessageStartingAction = "Starting Test", MessageEndAction = "End Test", MonitoredTypes = "*", ArgumentsLogLevel = LogLevel.INFO)]
	public IHttpActionResult LogInfoWithHttpGet_WarnTimeout(Models.ClientModel client)
	{
		System.Threading.Thread.Sleep(31000);
		return Ok("Success");
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:51:32.179] [10] DENUG - {"ContextId":"898a89d7-d01-4956-8c84-15f659460b34","MachineName":"F2ASRV","Controller":"Action4Log","Action":"LogInfoWithHttpGet_WarnTimeout","Method":"GET","UserName":"Anonymous"} - Starting Test
	// [20160505 19:51:32.241] [10] INFO -  {"MachineName":"F2ASRV","Controller":"Action4Log","Action":"LogInfoWithHttpGet_WarnTimeout","Method":"POST","UserName":"Anonymous"} ARGS {"WebApiFilters4Log.WebApiTest.Models.ClientModel client":"{'Id':'711192f5-a832-47e6-82cf-d2dda129f406','Name':'Jose Fulano','Years':35,'Emails':['joseFulano@teste.com','joseFulano2@gmail.com'],'Orders':[{'Date':'2016-02-25T11:27:05','Closed': true,'Items':[{'Product':'Arroz','Amount':2,'Value':13.23},{'Product':'Feijao','Amount':4,'Value':8.74}]}]}"}
	// [20160505 19:52:43.181] [10] WARN - {"ContextId":"898a89d7-dd01-4956-8c84-15f659460b34","WarnTimeout":"true","Time":"31,002"} - End Test
}
```

### **Arguments4LogFilter** - Aplicando a um controlador

```csharp
[Action4LogFilter("Logger")]
public class Arguments4LogController : ApiController
{
	[HttpGet]
	public IHttpActionResult LogPrimitiveTypes(int id, decimal value, string text)
	{
		return Ok();
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:51:40.252] [8] DEBUG - {"MachineName":"F2ASRV","Controller":"Arguments4Log","Action":"LogPrimitiveTypes","Method":"GET","UserName":"Anonymous"} ARGS {"System.Int32 id":"6","System.Decimal value":"2.34","System.String text":"'testing'"}

	[HttpPost]
	public IHttpActionResult LogComplexTypes(Models.ClientModel client)
	{
		return Ok();
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:52:04.837] [8] DEBUG - {"MachineName":"F2ASRV","Controller":"Arguments4Log","Action":"LogComplexTypes","Method":"POST","UserName":"Anonymous"} ARGS {"WebApiFilters4Log.WebApiTest.Models.ClientModel client":"{'Id':'711192f5-a832-47e6-82cf-d2dda129f406','Name':'Jose Fulano','Years':35,'Emails':['joseFulano@teste.com','joseFulano2@gmail.com'],'Orders':[{'Date':'2016-02-25T11:27:05','Closed':true,'Items':[{'Product':'Arroz','Amount':2,'Value':13.23},{'Product':'Feijao','Amount':4,'Value':8.74}]}]}"}

	[HttpPut]
	public IHttpActionResult LogInformedComplexTypes([FromUri] int id, [FromBody] Models.ClientModel client)
	{
		return Ok();
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:52:19.382] [9] WARN - {"MachineName":"F2ASRV","Controller":"Arguments4Log","Action":"LogInformedComplexTypes","Method":"PUT","UserName":"Anonymous"} ARGS {"System.Int32 id":"8","WebApiFilters4Log.WebApiTest.Models.ClientModel client":"{'Id':'711192f5-a832-47e6-82cf-d2dda129f406','Name':'Jose Fulano','Years':35,'Emails':['joseFulano@teste.com','joseFulano2@gmail.com'],'Orders':[{'Date':'2016-02-25T11:27:05','Closed':true,'Items':[{'Product':'Arroz','Amount':2,'Value':13.23},{'Product':'Feijao','Amount':4,'Value':8.74}]}]}"}
}
```

### **Arguments4LogFilter** - Aplicando a uma ação

```csharp
public class Arguments4LogController : ApiController
{
	[HttpGet]
	[Arguments4LogFilter("Logger")]
	public IHttpActionResult LogPrimitiveTypes(int id, decimal value, string text)
	{
		return Ok();
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:52.456] [10] DEBUG - {"MachineName":"F2ASRV","Controller":"Arguments4Log","Action":"LogPrimitiveTypes","Method":"GET","UserName":"Anonymous"} ARGS {"System.Int32 id":"6","System.Decimal value":"2.34","System.String text":"'testing'"}

	[HttpPost]
	[Arguments4LogFilter("Logger", LogLevel.INFO, "*")]
	public IHttpActionResult LogComplexTypes(Models.ClientModel client)
	{
		return Ok();
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:52.540] [10] INFO - {"MachineName":"F2ASRV","Controller":"Arguments4Log","Action":"LogComplexTypes","Method":"POST","UserName":"Anonymous"} ARGS {"WebApiFilters4Log.WebApiTest.Models.ClientModel client":"{'Id':'711192f5-a832-47e6-82cf-d2dda129f406','Name':'Jose Fulano','Years':35,'Emails':['joseFulano@teste.com','joseFulano2@gmail.com'],'Orders':[{'Date':'2016-02-25T11:27:05','Closed':true,'Items':[{'Product':'Arroz','Amount':2,'Value':13.23},{'Product':'Feijao','Amount':4,'Value':8.74}]}]}"}

	[HttpPut]
	[Arguments4LogFilter("Logger", LogLevel.WARN, "FullNamespace.Models.ClientModel")]
	public IHttpActionResult LogInformedComplexTypes([FromUri] int id, [FromBody] Models.ClientModel client)
	{
		return Ok();
	}
	// Resultado obtido utilizando o PatternLayout do log4net "[%date{yyyyMMdd HH:mm:ss.fff}] [%thread] %level - %message%newline":
	// [20160505 19:52.551] [9] WARN - {"MachineName":"F2ASRV","Controller":"Arguments4Log","Action":"LogInformedComplexTypes","Method":"PUT","UserName":"Anonymous"} ARGS {"WebApiFilters4Log.WebApiTest.Models.ClientModel client":"{'Id':'711192f5-a832-47e6-82cf-d2dda129f406','Name':'Jose Fulano','Years':35,'Emails':['joseFulano@teste.com','joseFulano2@gmail.com'],'Orders':[{'Date':'2016-02-25T11:27:05','Closed':true,'Items':[{'Product':'Arroz','Amount':2,'Value':13.23},{'Product':'Feijao','Amount':4,'Value':8.74}]}]}"}
}
```

### **Exception4LogFilter** - Tratamento do resultado da requisição simples e utilizando *ControllerException*

```csharp
public class Exception4LogController : ApiController
{
	// Tratamento do resultado da requisição de forma simples
	[HttpGet]
	[Exception4LogFilter("ExceptionLogger")]
	public IHttpActionResult LogSimpleException()
	{
		ThrowExceptionLevel1();
		return Ok(Models.ClientModel.GetFakeClient());
	}
	// Resultados para a requisição: GET /api/Exception4Log/LogSimpleException
	// Resposta: HTTP/1.1 500 Internal Server Error
	//
	// Conteúdo registrado em arquivo e retornado na resposta quando o cabeçalho X-DebugError=true é informado na requisição:
	//{
	//  "Id": "5eefb5d0-dd45-4310-a20d-3205a1e3fb82",
	//  "DateTime": "2016-05-05T19:24:01.6900723-03:00",
	//  "Message": "testando log...",
	//  "Source": "WebApiFilters4Log.WebApiTest",
	//  "StackTrace": "   em WebApiFilters4Log.WebApiTest.Controllers.Exception4LogController.ThrowExceptionLevel2() na C:\\Workspaces\\WebApiFilters4Log\\WebApiFilters4Log.WebApiTest\\Controllers\\Exception4LogController.cs:linha 53",
	//  "ExceptionType": "System.InvalidOperationException",
	//  "AdditionalInfo": {},
	//  "InnerException": null
	//}
	//
	// Conteúdo retornado quando o cabeçalho X-DebugError=true não é informado na requisição:
	//{
	//	"Message": "testando log...",
	//	"ErrorCode": 500,
	//	"ErrorId": "5eefb5d0-dd45-4310-a20d-3205a1e3fb82",
	//	"DateTime": "2016-05-05T19:24:01.69088"
	//}

	// Tratamento do resultado da requisição utilizando ControllerException e definindo uma chave de confiança para acessar os detalhes da exceção
	[HttpPost]
	[Exception4LogFilter("ExceptionLogger", "trustKey123", "DebugAPI")]
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
	// Resultados para a requisição: POST /api/Exception4Log/LogDetailedException HTTP/1.1
	// Resposta: HTTP/1.1 500 Internal Server Error
	//
	// Conteúdo registrado em arquivo e retornado na resposta quando o cabeçalho DebugAPI=trustKey123 é informado na requisição:
	//{
	//  "Id": "1ed6f8ba-e20a-42da-a06a-3de25e3e0392",
	//  "DateTime": "2016-05-05T19:46:04.2884895-03:00",
	//  "Message": "Ocorreu um erro inesperado executando Exception4Log.LogDetailedException!",
	//  "Source": "WebApiFilters4Log.WebApiTest",
	//  "StackTrace": "   em WebApiFilters4Log.WebApiTest.Controllers.Exception4LogController.LogDetailedException(ClientModel client) na C:\\Workspaces\\WebApiFilters4Log\\WebApiFilters4Log.WebApiTest\\Controllers\\Exception4LogController.cs:linha 27",
	//  "ExceptionType": "WebApiFilters4Log.ControllerException",
	//  "AdditionalInfo": {
	//    "Controller": "Exception4Log",
	//    "Action": "LogDetailedException",
	//    "PathAndQuery": "/api/Exception4Log/LogDetailedException",
	//    "User": null,
	//    "ContextId": "3c403cac-fc6a-470a-a433-2442014566bf",
	//    "WebApiFilters4Log.WebApiTest.Models.ClientModel client": "{'Id':'711192f5-a832-47e6-82cf-d2dda129f406','Name':'Jose Fulano','Years':35,'Emails':['joseFulano@teste.com','joseFulano2@gmail.com'],'Orders':[{'Date':'2016-02-29T15:08:45.0024138-03:00','Closed':true,'Items':[{'Product':'Arroz','Amount':2,'Value':13.23},{'Product':'Feijão','Amount':4,'Value':8.74}]}]}"
	//  },
	//  "InnerException": {
	//    "Id": "1ed6f8ba-e20a-42da-a06a-3de25e3e0392",
	//    "DateTime": "2016-05-05T19:46:04.2894898-03:00",
	//    "Message": "testando log...",
	//    "Source": "WebApiFilters4Log.WebApiTest",
	//    "StackTrace": "   em WebApiFilters4Log.WebApiTest.Controllers.Exception4LogController.ThrowExceptionLevel2() na C:\\Workspaces\\WebApiFilters4Log\\WebApiFilters4Log.WebApiTest\\Controllers\\Exception4LogController.cs:linha 53\r\n   em WebApiFilters4Log.WebApiTest.Controllers.Exception4LogController.ThrowExceptionLevel1() na C:\\Workspaces\\WebApiFilters4Log\\WebApiFilters4Log.WebApiTest\\Controllers\\Exception4LogController.cs:linha 49\r\n   em WebApiFilters4Log.WebApiTest.Controllers.Exception4LogController.LogDetailedException(ClientModel client) na C:\\Workspaces\\WebApiFilters4Log\\WebApiFilters4Log.WebApiTest\\Controllers\\Exception4LogController.cs:linha 23",
	//    "ExceptionType": "System.InvalidOperationException",
	//    "AdditionalInfo": {},
	//    "InnerException": null
	//  }
	//}
	//
	// Conteúdo retornado quando o cabeçalho DebugAPI=trustKey123 não é informado na requisição:
	//{
	//  "Message": "Ocorreu um erro inesperado executando Exception4Log.LogDetailedException!",
	//  "ErrorCode": 500,
	//  "ErrorId": "1ed6f8ba-e20a-42da-a06a-3de25e3e0392",
	//  "DateTime": "05/05/2016 19:46:04.28905"
	//}

	private void ThrowExceptionLevel1()
	{
		ThrowExceptionLevel2();
	}

	private void ThrowExceptionLevel2()
	{
		throw new InvalidOperationException("testando log...");
	}
}
```
