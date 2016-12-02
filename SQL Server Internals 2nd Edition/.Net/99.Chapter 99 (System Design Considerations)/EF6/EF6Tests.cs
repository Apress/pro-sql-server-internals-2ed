/****************************************************************************/
/*                       Pro SQL Server Internals                           */
/*      APress. 1st Edition. ISBN-13: 978-1430259626 ISBN-10:1430259620     */
/*                                                                          */
/*                  Written by Dmitri V. Korotkevitch                       */
/*                      http://aboutsqlserver.com                           */
/*                      dmitri@aboutsqlserver.com                           */
/****************************************************************************/
/*                   Chapter 16. System Design Considerations               */
/*                       C# code written by Maxim Alexeyev                  */
/*                       http://discoveringdotnet.alexeyev.org              */
/****************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ORMTests.Model;

namespace ORMTests.EF6
{
	public static class EF6Tests
	{
		/// <summary>
		/// ! Test types of the variables
		/// </summary>
		public static void QueryCustomersCreditLimitFilter()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				// constant
				var q = context.Customers.Where(t => t.CreditLimit == 10).Select(t => t.FirstName);
				var result = q.ToList();

				int i = 10;
				q = context.Customers.Where(t => t.CreditLimit == i).Select(t => t.FirstName);
				result = q.ToList();

				long l = 11;
				q = context.Customers.Where(t => t.CreditLimit == l).Select(t => t.FirstName);
				result = q.ToList();

				byte b = 12;
				q = context.Customers.Where(t => t.CreditLimit == b).Select(t => t.FirstName);
				result = q.ToList();

			}
		}

		/// <summary>
		/// ! String filtering
		/// </summary>
		public static void QueryCustomersNameFilter()
		{
			using (var context = new EFDbContext())
			{
				// constant
				var q = context.Customers.Where(t => t.FirstName == "John").Select(t => new { t.FirstName, t.LastName });
				var sql = q.ToString();
				Debug.WriteLine(sql);
				var result = q.ToList();

				string name = "John";
				q = context.Customers.Where(t => t.FirstName == name).Select(t => new { t.FirstName, t.LastName });
				sql = q.ToString();
				Debug.WriteLine(sql);
				result = q.ToList();
			}
		}

		/// <summary>
		/// Test opening/closing connections
		/// </summary>
		public static void OpeningConnections()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				var q = context.Orders.ToList();
				foreach (var customer in context.Customers)
				{
					Trace.WriteLine(string.Format("Customer Id: {0}", customer.CustomerId));
					Thread.Sleep(10);
				}
			}
		}

		/// <summary>
		/// ! Test varchar vs nvarchar
		/// </summary>
		public static void QueryCustomersEmailFilter()
		{
			using (var context = new EFDbContext())
			{
				// constant
				var q = context.Customers.Where(t => t.Email == "a@gmail.com").Select(t => t.FirstName);
				var result = q.ToList();

				string email = "a@gmail.com";

				q = context.Customers.Where(t => t.Email == email).Select(t => t.FirstName);
				result = q.ToList();


				q = context.Customers.Where(t => t.Email == EntityFunctions.AsNonUnicode(email)).Select(t => t.FirstName);
				result = q.ToList();
			}
		}

		public static void QueryCustomersLastPurchaseDateFilter()
		{
			using (var context = new EFDbContext())
			{
				DateTime date = new DateTime(2013, 1, 2);

				var q = context.Customers.Where(t => t.LastPurchaseDate == date);
				var sql = q.ToString();
				Debug.WriteLine(sql);
				var result = q.FirstOrDefault();
			}
		}

		/// <summary>
		/// ! Query list: select ... where ... IN ()
		/// </summary>
		public static void QueryInFilter()
		{
			var list = new List<int>();
			for (int i = 1; i < 100; i++)
				list.Add(i);
			using (var context = new EFDbContext())
			{
				var q = context.Customers.Where(t => list.Contains(t.CustomerId)).Select(t => t.FirstName);
				var result = q.ToList();
			}
		}

		/// <summary>
		/// Updates
		/// </summary>
		public static void PopulateCustomer()
		{
			var firstName = "First";
			var lastName = "Last";
			int start = 3;
			int count = 1000;
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				if (context.Customers.Count() > 0)
					return;
				for (int i = start; i < start + count; i++)
				{
					var customer = new Customer();
					customer.FirstName = string.Format("{0}_{1}", firstName, i);
					customer.LastName = string.Format("{0}_{1}", lastName, i);
					customer.Created = DateTime.Now;
					customer.Modified = DateTime.Now;
					context.Customers.Add(customer);
				}
				context.SaveChanges();
			}
		}

		/// <summary>
		/// Separate calls to SaveChanges under same transaction
		/// </summary>
		public static void AddOrder()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				using (var transaciton = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					try
					{
						Trace.WriteLine(transaciton.UnderlyingTransaction.IsolationLevel);
						var order = new Order();
						var customer = context.Customers.OrderBy(t => t.CustomerId).Skip(3).First();
						order.Customer = customer;
						order.OrderNo = "1";
						order.Modified = DateTime.Now;
						order.Created = DateTime.Now;
						var item = new OrderItem();
						item.Qty = 1;
						item.Price = 10;
						order.Items.Add(item);
						context.Orders.Add(order);
						context.SaveChanges();

						order = new Order();
						order.Customer = customer;
						order.CustomerId = 1;
						order.OrderNo = DateTime.Now.ToString();
						order.Modified = DateTime.Now;
						order.Created = DateTime.Now;

						item = new OrderItem();
						item.Qty = 1;
						item.Price = 10;
						order.Items.Add(item);
						context.Orders.Add(order);
						context.SaveChanges();
						transaciton.Commit();
						Trace.WriteLine("Commited");
					}
					catch
					{
						transaciton.Rollback();
						Trace.WriteLine("Rollbacked");
					}
				}
			}
		}


		/// <summary>
		/// Reatrieving data under transaction
		/// </summary>
		public static void ReadCustomer()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				using (var transaciton = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					try
					{
						var customer = context.Customers.OrderByDescending(t => t.CustomerId).First();
						Trace.WriteLine(string.Format("Customer: {0}", customer.FirstName));
						transaciton.Commit();
						Trace.WriteLine("Commited");
					}
					catch
					{
						transaciton.Rollback();
						Trace.WriteLine("Rollbacked");
					}
				}

				using (var transaciton = context.Database.BeginTransaction())
				{
					try
					{
						var customer = context.Customers.OrderByDescending(t => t.CustomerId).First();
						Trace.WriteLine(string.Format("Customer: {0}", customer.FirstName));
						transaciton.Commit();
						Trace.WriteLine("Commited");
					}
					catch
					{
						transaciton.Rollback();
						Trace.WriteLine("Rollbacked");
					}
				}
			}
		}

		/// <summary>
		/// ! Testing concurrency (two instances should be ran)
		/// </summary>
		public static void ModifyCustomer()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				var customer = context.Customers.OrderByDescending(t => t.CustomerId).First();
				Trace.WriteLine(string.Format("Customer: Id: {0} Name {1}", customer.CustomerId, customer.FirstName));
				customer.FirstName = string.Format("Modified at {0}", DateTime.Now);
				customer.Modified = DateTime.Now;
				Thread.Sleep(5000);
				try
				{
					context.SaveChanges();
				}
				catch (DbUpdateConcurrencyException ex)
				{
					// whatever response you need to provide to the user
					foreach (var item in ex.Entries)
						item.Reload();
				}
			}
		}


		public static void CheckConnection()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				Trace.WriteLine(context.Database.Connection.State);
				using (var tr = context.Database.BeginTransaction())
				{
					try
					{
						var customer = context.Customers.First();
						for (int i = 0; i < 10; i++)
						{
							Thread.Sleep(100);
							Trace.WriteLine(context.Database.Connection.State);
						}
						tr.Commit();
					}
					catch
					{
						tr.Rollback();
					};
				}
			}
		}

		/// <summary>
		/// ! Tests for lazyloading
		/// </summary>
		/// <param name="lazyLoading"></param>
		public static void ReadOrder(bool lazyLoading)
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				var order = context.Orders.Include("Customer").Include("Items").Where(t => t.OrderNo == "1").First(); // we know there should be one
				Trace.WriteLine(string.Format("Order Id: {0} No:{1} fetched", order.OrderId, order.OrderNo));
				var customer = order.Customer;
				Trace.WriteLine(string.Format("Customer Id:{0} Name:{1}", order.CustomerId, customer.FirstName + " " + customer.LastName));
				if (!lazyLoading)
					context.Entry(order).Collection(t => t.Items).Load();
				foreach (var item in order.Items)
				{
					Trace.WriteLine(string.Format("Item Id:{0} Price: {1}", item.OrderId, item.Price));
				}
			}
		}


		/// <summary>
		/// Deleting when refernces involved
		/// </summary>
		public static void DeleteCustomer()
		{
			using (var context = new EFDbContext())
			{

				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				var customer = new Customer();
				customer.FirstName = "Test";
				customer.LastName = "Delete";
				customer.Created = DateTime.Now;
				customer.Modified = DateTime.Now;
				context.Customers.Add(customer);

				var order = new Order();
				order.OrderNo = DateTime.Now.ToString();
				order.Customer = customer;
				order.Created = DateTime.Now;
				order.Modified = DateTime.Now;

				var orderItem = new OrderItem();
				order.Items.Add(orderItem);

				orderItem = new OrderItem();
				order.Items.Add(orderItem);

				context.Orders.Add(order);

				order = new Order();
				order.OrderNo = "2:" + DateTime.Now.ToString();
				order.Customer = customer;
				order.Created = DateTime.Now;
				order.Modified = DateTime.Now;

				orderItem = new OrderItem();
				order.Items.Add(orderItem);
				context.Orders.Add(order);

				context.SaveChanges();

				context.Customers.Remove(customer);
				context.SaveChanges();

			}

		}

		/// <summary>
		/// Deleting without loading entity first
		/// </summary>
		public static void DeleteCustomerAttach()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				var c = context.Customers.First().CustomerId;

				var customer = new Customer();
				customer.CustomerId = c;
				context.Customers.Attach(customer);

				context.Customers.Remove(customer);
				context.SaveChanges();
			}

		}

		/// <summary>
		/// Deleting with custom sql
		/// </summary>
		public static void DeleteOrder()
		{
			using (var context = new EFDbContext())
			{
				context.Database.Log = (s) => Trace.WriteLine(s);
				context.Database.Connection.StateChange += Connection_StateChange;
				var order = context.Orders.First();
				context.Entry(order).Collection(t => t.Items).Load();
				context.Database.ExecuteSqlCommand(
@"delete from ef6.OrderItems where OrderId = @OrderId
delete from ef6.Orders where OrderId = @OrderId", new SqlParameter("OrderId", SqlDbType.Int) { Value = order.OrderId });

				context.Entry(order).State = System.Data.Entity.EntityState.Detached;
				context.SaveChanges();
			}

		}

		static void Connection_StateChange(object sender, StateChangeEventArgs e)
		{
			Trace.WriteLine(string.Format("Connection changed from {0} to {1}", e.OriginalState, e.CurrentState));
		}

	}
}
