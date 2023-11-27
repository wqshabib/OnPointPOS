using System;

namespace POSSUM.ApiModel
{
    public class CCUDataRequest
    {
        public string country { get; set; }
        public string corporateId { get; set; }
        public string cashRegisterName { get; set; }
        public long validFrom { get; set; }
        public long validTo { get; set; }
        public string[] features { get; set; }
        public string comment { get; set; }
        public ContactInfo contactInfo { get; set; }
        public string controlUnitSerial { get; set; }
        public string controlUnitLocation { get; set; }
        public ControlUnitGeolocation controlUnitGeolocation { get; set; }
        public RegistrationGeolocation registrationGeolocation { get; set; }
        public string applicationPackage { get; set; }
        public string productionNumber { get; set; }
        public InstallationCreationInfo installationCreationInfo { get; set; }
        public string connectionCode { get; set; }
        public string applicationNameAndVersion { get; set; }
    }

    public class ContactInfo
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }

    public class ControlUnitGeolocation
    {
        public string address { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string companyName { get; set; }
    }

    public class RegistrationGeolocation
    {
        public string address { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string companyName { get; set; }
    }

    public class InstallationCreationInfo
    {
        public string deviceId { get; set; }
        public BuildInfo buildInfo { get; set; }
    }

    public class BuildInfo
    {
        
    }
}
