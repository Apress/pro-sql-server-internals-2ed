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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORMTests.Model;

namespace ORMTests.EF6.Mapping
{
	internal class CustomerMap : EntityTypeConfiguration<Customer>
	{
		public CustomerMap()
		{
			HasKey(t => t.CustomerId);
			Property(t => t.FirstName).HasMaxLength(235);
			Property(t => t.LastName).HasMaxLength(255);
			Property(t => t.CustomerId).HasColumnType("int");
			Property(t => t.CreditLimit).HasColumnType("int");
			Property(t => t.LastPurchaseDate).HasColumnType("datetime");
			Property(t => t.Email).IsUnicode(false);
			Property(t => t.Email).HasMaxLength(254);
			Property(t => t.Ver)
				.IsConcurrencyToken()
				.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
		}


	}
}
