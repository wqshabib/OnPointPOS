using System;

namespace POSSUM.ApiModel
{
    [Serializable]
    public class LoginApi
    {
        public string grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string imei { get; set; }
    }
}