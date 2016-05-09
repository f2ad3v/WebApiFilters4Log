namespace WebApiFilters4Log
{
	using Newtonsoft.Json;
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;

	/// <summary>
	/// Customizacao de HttpContent
	/// </summary>
	public class JsonContent : HttpContent
	{
		private readonly MemoryStream _stream = new MemoryStream();

		/// <summary>
		/// Construtor que recebe um objeto que sera retornado como json
		/// </summary>
		/// <param name="value">Instancia de um objeto</param>
		/// <param name="formatting">Formato do json</param>
		public JsonContent(object value, Formatting formatting = Formatting.Indented)
		{
			if (value == null) throw new ArgumentNullException("value");

			var jw = new JsonTextWriter(new StreamWriter(_stream)) { Formatting = formatting };

			var serializer = new JsonSerializer();

			serializer.Serialize(jw, value);

			jw.Flush();

			_stream.Position = 0;
		}

		/// <summary>
		/// SerializeToStreamAsync
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
		{
			_stream.CopyTo(stream);

			var tcs = new TaskCompletionSource<object>();

			tcs.SetResult(null);

			return tcs.Task;
		}

		/// <summary>
		/// TryComputeLength
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		protected override bool TryComputeLength(out long length)
		{
			length = _stream.Length;

			return true;
		}

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing">disposing</param>
		protected override void Dispose(bool disposing)
		{
			_stream.Close();

			_stream.Dispose();

			base.Dispose(disposing);
		}
	}
}
