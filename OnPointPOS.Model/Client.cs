using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    
    public class Client : BaseEntity
    {
        public int Id { get; set; }
        public virtual string clientId { get; set; }

        public virtual long? clientRequestTimeout { get; set; }

        public virtual string password { get; set; }

        public virtual string connectionString { get; set; }
        public virtual string ClientUserId { get; set; }
        public virtual DateTime Updated { get; set; }
	}
}