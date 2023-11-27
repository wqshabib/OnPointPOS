namespace POSSUM.Web.Models
{
    public class SoftPayViewModel
    {
        public string SoftPayId { get; set; } = string.Empty;
        public string SoftPayMerchant { get; set; } = string.Empty;
        public string IntegratorCredentials { get; set; } = string.Empty;
        public bool SoftPayStatus { get; set; } = false;
    }
}
