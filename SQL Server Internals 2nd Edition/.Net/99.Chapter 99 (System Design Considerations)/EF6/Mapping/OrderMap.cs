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
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORMTests.Model;

namespace ORMTests.EF6.Mapping
{
	internal class OrderMap : EntityTypeConfiguration<Order>
	{
		public OrderMap()
		{
			HasKey(t => t.OrderId);
			Property(t => t.OrderNo).HasMaxLength(50);
		}

	}
}
