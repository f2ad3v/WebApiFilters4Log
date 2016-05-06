namespace WebApiFilters4Log.Test
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	public class LogInfo
	{
		public DateTime? DateTimeLog { get; set; }
		public string LogLevel { get; set; }
		public string Message { get; set; }
		public Dictionary<string, string> Context { get; set; }

		public LogInfo(string logLine)
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

			var dic = rest.Substring(0, rest.IndexOf("} -") + 1);

			Context = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(dic);

			Message = rest.Remove(0, rest.IndexOf("} -") + 4);
		}
	}
}
