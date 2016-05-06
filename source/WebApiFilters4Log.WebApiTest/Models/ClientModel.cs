namespace WebApiFilters4Log.WebApiTest.Models
{
	using System;
	using System.Collections.Generic;

	public class ClientModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int Years { get; set; }
		public List<string> Emails { get; set; }
		public List<OrderModel> Orders { get; set; }

		#region GetFakeClient

		public static ClientModel GetFakeClient()
		{
			var client = new ClientModel
			{
				Id = new Guid("711192f5-a832-47e6-82cf-d2dda129f406"),
				Name = "Jose Fulano",
				Years = 35,
				Emails = new List<string>
				{
					"joseFulano@teste.com",
					"joseFulano2@gmail.com",
				},
				Orders = new List<OrderModel>
				{
					new OrderModel
					{
						Closed = true,
						Date = new DateTime(2016, 02, 25, 11, 27, 05),
						Items = new List<OrderItemModel>
						{
							new OrderItemModel
							{
								Product = "Arroz",
								Amount = 2,
								Value = 13.23M
							},
							new OrderItemModel
							{
								Product = "Feijao",
								Amount = 4,
								Value = 8.74M
							},
							new OrderItemModel
							{
								Product = "Ovo",
								Amount = 5,
								Value = 3.94M
							}
						}
					},
					new OrderModel
					{
						Closed = false,
						Date = new DateTime(2016, 04, 30, 19, 48, 31),
						Items = new List<OrderItemModel>
						{
							new OrderItemModel
							{
								Product = "Cerveja",
								Amount = 12,
								Value = 3.52M
							}
						}
					}
				}
			};

			return client;
		}

		#endregion GetFakeClient
	}
}
