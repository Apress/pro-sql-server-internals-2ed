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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTests.Model
{
	public class Order
	{
		public Order()
		{
			Items = new HashSet<OrderItem>();
		}

		public int OrderId { get; set; }

		public int CustomerId { get; set; }

		public virtual Customer Customer { get; set; }

		public string OrderNo { get; set; }

		public DateTime Created { get; set; }

		public DateTime Modified { get; set; }

		public virtual ICollection<OrderItem> Items { get; set; }
	}
}
