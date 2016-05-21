namespace WebApiFilters4Log.Test
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	public class LogArgsInfo
	{
		public DateTime? DateTimeLog { get; set; }
		public string LogLevel { get; set; }
		public Dictionary<string, string> Context { get; set; }
		public Dictionary<string, string> Arguments { get; set; }

		public LogArgsInfo(string logLine, string argumentsMessage = "ARGS")
		{
			var dtStr = logLine.Substring(1, logLine.IndexOf(']') - 1);

			DateTime dtLog;
			if (DateTime.TryParseExact(dtStr, "yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtLog))
				DateTimeLog = dtLog;
			else
				DateTimeLog = null;

			var rest = logLine.Remove(0, logLine.IndexOf(']') + 2);
			rest = rest.Remove(0, rest.IndexOf(']') + 2);

			LogLevel = rest.Substring(0, rest.IndexOf(" -"));

			rest = rest.Remove(0, rest.IndexOf("- ") + 2);

			string halfMessage = string.Concat("} ", argumentsMessage, " {");

			var dicContext = rest.Substring(0, rest.IndexOf(halfMessage) + 1);

			Context = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(dicContext);

			rest = rest.Remove(0, rest.IndexOf(halfMessage) + argumentsMessage.Length + 3);

			Arguments = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(rest);
		}
	}
}
