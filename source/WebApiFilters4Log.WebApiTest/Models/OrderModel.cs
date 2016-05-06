namespace WebApiFilters4Log.WebApiTest.Models
{
	using System;
	using System.Collections.Generic;

	public class OrderModel
	{
		public DateTime Date { get; set; }
		public bool Closed { get; set; }
		public List<OrderItemModel> Items { get; set; }
	}
}
