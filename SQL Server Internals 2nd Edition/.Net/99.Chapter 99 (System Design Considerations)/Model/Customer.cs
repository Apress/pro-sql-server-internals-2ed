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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTests.Model
{
	public class Customer
	{
		public Customer()
		{
			Orders = new HashSet<Order>();
		}

		public int CustomerId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public DateTime Created { get; set; }

		public DateTime Modified { get; set; }

		public DateTime? LastPurchaseDate { get; set; }

		public int? CreditLimit { get; set; }

		//[Timestamp]
		public byte[] Ver { get; set; }

		public virtual ICollection<Order> Orders { get; set; }

	}
}
