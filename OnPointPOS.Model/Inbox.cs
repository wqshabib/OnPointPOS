using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
	public class Inbox:BaseEntity
	{
		public virtual long Id { get; set; }
		public virtual string Description { get; set; }
		public virtual DateTime CreatedOn { get; set; }
		public virtual bool IsRead { get; set; }
	}
}
