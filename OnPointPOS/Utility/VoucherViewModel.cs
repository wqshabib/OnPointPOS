using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace POSSUM
{
    public class VoucherViewModel
    {
        public string ImageTage { get; set; }

        public string Description { get; set; }
        public string ProductEAN { get; set; }
        public string ProductName { get; set; }
        public string BarCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string ImagePath
        {
            get
            {
                return GetImagePath();
            }
        }

        public string Serial { get; internal set; }

        private string GetImagePath()
        {
            try
            {
                var ind1 = ImageTage.IndexOf("=");
                var ind2 = ImageTage.IndexOf(".");
                var length = ind2 - ind1;
                var imgName = ImageTage.Substring(ind1+2, length-2);
                var ext = ImageTage.Substring(ind2, 4);
                string path = AppDomain.CurrentDomain.BaseDirectory + "Logos/" + imgName + ext;
                return path;
            }
            catch
            {
                return "";
            }
        }
    }
}
