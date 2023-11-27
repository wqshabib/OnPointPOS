using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class ContentPush
    {
        public Guid ContentPushId { get; set; }
        public string Message { get; set; }
        public DateTime PushDateTime { get; set; }


    }



}