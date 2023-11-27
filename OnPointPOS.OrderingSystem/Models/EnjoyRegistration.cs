using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class EnjoyRegistration : Result
    {
        public int RegistrationStatus { get; set; }
        public string Product { get; set; }
    }

}