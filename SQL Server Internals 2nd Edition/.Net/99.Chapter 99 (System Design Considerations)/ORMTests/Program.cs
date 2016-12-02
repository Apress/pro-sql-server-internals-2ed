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
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORMTests.EF6;

namespace ORMTests
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Database.SetInitializer<EFDbContext>(null); // To remove unncesessary queries when data context is initialized
				EF6Tests.PopulateCustomer();
				GetCustomersTests();
			}

			catch (Exception ex)
			{
				var msg = GetExceptionMsg(ex);
				Console.WriteLine(msg);
				Trace.WriteLine(msg);
				if (ex is DbEntityValidationException)
				{
					foreach (var error in ((DbEntityValidationException)ex).EntityValidationErrors)
					{
						msg = string.Format("Entity: {0}", error.Entry.Entity);
						Trace.WriteLine(msg);
						foreach (var dbError in error.ValidationErrors)
						{
							msg = string.Format("  Property: {0} Error:{1}", dbError.PropertyName, dbError.ErrorMessage);
							Trace.WriteLine(msg);
						}
					}
				}
				Console.ReadLine();
			}
		}

		private static void GetCustomersTests()
		{
			Trace.WriteLine("starting");
			EF6Tests.QueryCustomersNameFilter();
			EF6Tests.QueryCustomersEmailFilter();
			EF6Tests.QueryCustomersLastPurchaseDateFilter();

			EF6Tests.QueryInFilter();

			EF6Tests.ModifyCustomer();

			EF6Tests.AddOrder();
			EF6Tests.ReadOrder(false);
			EF6Tests.OpeningConnections();
			EF6Tests.DeleteOrder();
		}

		private static string GetExceptionMsg(Exception ex)
		{
			var e = ex;
			var sb = new StringBuilder();
			do
			{
				sb.AppendLine(e.Message);
				e = e.InnerException;
			}
			while (e != null);
			sb.AppendLine(ex.StackTrace);
			return sb.ToString();
		}
	}
}
