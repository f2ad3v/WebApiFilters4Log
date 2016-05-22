namespace WebApiFilters4Log
{
	using System;

	/// <summary>
	/// Excecao padrao
	/// </summary>
	[Serializable]
	public class CommonException : Exception
	{
		/// <summary>
		/// Construtor para uma nova instância de CommonException
		/// </summary>
		/// <param name="message">Mensagem da exceção</param>
		/// <param name="id">Identificador da exceção</param>
		/// <param name="innerException">Exceção capturada</param>
		public CommonException(string message, Guid id, Exception innerException)
			: base(message, innerException)
		{
			Id = id;

			DateTimeError = DateTime.Now;
		}

		/// <summary>
		/// Identificador global da exceção
		/// </summary>
		public readonly Guid Id;

		/// <summary>
		/// Data e hora da exceção
		/// </summary>
		public readonly DateTime DateTimeError;
	}
}
