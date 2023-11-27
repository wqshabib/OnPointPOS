using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ML.Common
{
    public class SmsHelper
    {
        public static string CleanPhoneNumber(string strPhoneNo)
        {
            try
            {

                string strText = string.Empty;

                // Break when number is empty
                if (string.IsNullOrEmpty(strPhoneNo))
                {
                    return string.Empty;
                }
                else
                {
                    // Clean
                    strText = CleanNumber(strPhoneNo);
                }

                if (strText == string.Empty)
                {
                    return string.Empty;
                }

                // Break when phonenumber is way to short
                if (strText.Length < 5)
                {
                    return string.Empty;
                }

                // Handle Swedish numbers
                if (strText.Substring(0, 1) == "0" || strText.Substring(0, 3) == "460")
                {
                    strText = strText.TrimStart("0".ToCharArray());

                    // Break when number is not a valid mobile phone number
                    if (strText.Substring(0, 1) != "7" && strText.Substring(0, 2) != "46")
                    {
                        return string.Empty;
                    }

                    // Correct number
                    if (strText.Length > 2)
                    {
                        if (strText.Substring(0, 2) == "46" && strText.Substring(2, 1) == "0")
                        {
                            strText = strText.TrimStart("460".ToCharArray());
                        }
                    }

                    if (strText.Length > 1)
                    {
                        if (strText.Substring(0, 2) != "46")
                        {
                            strText = strText.Insert(0, "46");
                        }
                    }
                }

                if (!Text.IsNumeric(strText))
                {
                    strText = string.Empty;
                }
                // Cont...

                return strText;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string CleanNumber(string strNumber)
        {
            string strText = strNumber.Replace(" ", string.Empty);
            strText = ML.Common.Text.ReplaceEx(strText, "-", string.Empty);
            strText = ML.Common.Text.ReplaceEx(strText, "+", string.Empty);
            strText = ML.Common.Text.ReplaceEx(strText, " ", string.Empty);
            
            if (!Text.IsNumeric(strText))
            {
                strText = string.Empty;
            }
            // Cont...

            return strText;
        }

        public static int GetNoOfConcatenations(string strMessageData)
        {
            int intNoOf = 0;
            if (strMessageData.Length > 160)
            {
                for (int i = 0; i < strMessageData.Length; i++)
                {
                    if (i % 153 == 0)
                    {
                        intNoOf++;
                    }
                }
            }
            else
            {
                intNoOf++;
            }

            return intNoOf;
        }




    }
}
