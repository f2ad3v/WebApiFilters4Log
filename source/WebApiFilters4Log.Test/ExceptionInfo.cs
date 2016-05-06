namespace WebApiFilters4Log.Test
{
	using System;
	using System.Collections.Generic;

	public class ExceptionInfo
	{
		public Guid Id { get; set; }
		public DateTime DateTime { get; set; }
		public string Message { get; set; }
		public string ExceptionType { get; set; }
		public string Source { get; set; }
		public string StackTrace { get; set; }
		public Dictionary<string, string> AdditionalInfo { get; set; }
		public ExceptionInfo InnerException { get; set; }
	}
}
