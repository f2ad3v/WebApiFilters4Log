namespace WebApiFilters4Log
{
	using System;

	/// <summary>
	/// Excecao padrao
	/// </summary>
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
			DateTimeError = DateTime.Now;
			Id = id;
		}

		/// <summary>
		/// Construtor para uma nova instância de CommonException
		/// </summary>
		/// <param name="message">Mensagem da exceção</param>
		/// <param name="id">Identificador da exceção</param>
		public CommonException(string message, Guid id)
			: this(message, id, null)
		{
		}

		/// <summary>
		/// Construtor para uma nova instância de CommonException
		/// </summary>
		/// <param name="message">Mensagem da exceção</param>
		public CommonException(string message)
			: this(message, Guid.NewGuid())
		{
		}

		/// <summary>
		/// Construtor para uma nova instância de CommonException
		/// </summary>
		public CommonException()
			: this(string.Empty)
		{
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
