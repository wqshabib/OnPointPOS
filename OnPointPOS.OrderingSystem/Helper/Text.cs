using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ML.Common
{
    public class Text
    {
        public static bool IsNumeric(object Expression)
        {
            bool bNum;
            double retNum;
            bNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return bNum;
        }

        public static bool IsDateTime(object Expression)
        {
            try
            {
                if (Expression == null)
                {
                    return false;
                }
                DateTime dt = Convert.ToDateTime(Expression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsBoolean(object Expression)
        {
            try
            {
                int intBool = Convert.ToInt32(Expression);
                bool b = Convert.ToBoolean(intBool);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGuid(object Expression)
        {
            try
            {
                Guid guid = new Guid(Expression.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGuidNotEmpty(object Expression)
        {
            try
            {
                Guid guid = new Guid(Expression.ToString());
                if (guid != Guid.Empty)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool StartsWithGuidNotEmpty(object Expression, string strDelimter)
        {
            try
            {
                string[] str = Expression.ToString().Split(Convert.ToChar(strDelimter));

                Guid guid = new Guid(str[0]);
                if (guid != Guid.Empty)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGuidEmpty(object Expression)
        {
            try
            {
                Guid guid = new Guid(Expression.ToString());
                if (guid == Guid.Empty)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string ParseText(object Expression)
        {
            return Expression == null ? string.Empty : Expression.ToString();
        }

        public static bool AlgorithmMatch(string strAlgorithm, string strText)
        {
            try
            {
                string[] strSplitAlgorithm = strAlgorithm.Split(Convert.ToChar(" "));
                string[] strSplitText = strText.Split(Convert.ToChar(" "));

                if (strSplitAlgorithm.Length != strSplitText.Length)
                {
                    return false;
                }

                for (int i = 0; i < strSplitAlgorithm.Length; i++)
                {
                    if (strSplitAlgorithm[i] == "#" && !ML.Common.Text.IsNumeric(strSplitText[i]))
                    {
                        return false;
                    }
                    else if (strSplitAlgorithm[i] == "$" && ML.Common.Text.IsNumeric(strSplitText[i]))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static string AlgorithmRevice(string strText)
        {
            if (string.IsNullOrEmpty(strText))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            string[] strSplitText = strText.Split(Convert.ToChar(" "));

            for (int i = 0; i < strSplitText.Length; i++)
            {
                if (ML.Common.Text.IsNumeric(strSplitText[i]))
                {
                    sb.Append("#");
                    if (i < strSplitText.Length - 1)
                    {
                        sb.Append(" ");
                    }
                }
                else if (!ML.Common.Text.IsNumeric(strSplitText[i]))
                {
                    sb.Append("$");
                    if (i < strSplitText.Length - 1)
                    {
                        sb.Append(" ");
                    }
                }
            }

            return sb.ToString();
        }

        //public static bool IsEmptyOrGuidEmpty(object Expression)
        //{
        //    try
        //    {
        //        Guid guid = new Guid(Expression.ToString());
        //        if (guid != Guid.Empty)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static string ReplaceEx(string strOriginal, string strPattern, string strReplacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = strOriginal.ToUpper();
            string upperPattern = strPattern.ToUpper();
            int inc = (strOriginal.Length / strPattern.Length) *
                      (strReplacement.Length - strPattern.Length);
            char[] chars = new char[strOriginal.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = strOriginal[i];
                for (int i = 0; i < strReplacement.Length; ++i)
                    chars[count++] = strReplacement[i];
                position0 = position1 + strPattern.Length;
            }
            if (position0 == 0) return strOriginal;
            for (int i = position0; i < strOriginal.Length; ++i)
                chars[count++] = strOriginal[i];
            return new string(chars, 0, count);
        }

        public static string TruncateWords(string strText, int intLengthThreshold, int intNewWordLength, string strTruncatedNewWordSuffix)
        {
            if (strText.Length < intLengthThreshold)
            {
                return strText;
            }

            string[] words = strText.Split(' ');

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > intNewWordLength + 1)
                {
                    sb.Append(words[i].Substring(0, intNewWordLength));

                    if (!string.IsNullOrEmpty(strTruncatedNewWordSuffix))
                    {
                        sb.Append(strTruncatedNewWordSuffix);
                    }
                }
                else
                {
                    sb.Append(words[i]);
                }

                sb.Append(" ");
            }

            return sb.ToString();
        }

        public static string FormatNumber(decimal decNumber)
        {
            return FormatNumber(decNumber, 0);
        }

        public static string FormatNumber(decimal decNumber, int intNoOfDecimals)
        {
            string strNumber = string.Empty;

            switch (intNoOfDecimals)
            {
                case 0:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0");
                    break;
                case 1:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0.#");
                    break;
                case 2:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0.##");
                    break;
                case 3:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0.###");
                    break;
                default:
                    strNumber = decNumber.ToString();
                    break;
            }

            return strNumber.Trim();
        }

        public static string Truncate(string strText, string strStart, string strEnd)
        {
            int intStartPos = strText.IndexOf(strStart, 0);
            if (intStartPos == -1)
            {
                return strText;
            }
            int intEndPos = strText.IndexOf(strEnd, intStartPos);
            if (intEndPos == -1)
            {
                return strText;
            }
            return ReplaceEx(strText, strText.Substring(intStartPos, intEndPos - intStartPos + 1), string.Empty).Trim();
        }

        public static string DateTimeToFileFormat(DateTime dt)
        {
            string strDt = dt.ToString();
            strDt = ReplaceEx(strDt, ":", "_");

            return strDt;
        }

        public static string AnyDateTimeToYearAndMonth(object dateTime)
        {
            DateTime dt = Convert.ToDateTime(dateTime);
            return string.Concat(dt.Year.ToString(), "-", dt.Month.ToString().PadLeft(2, '0'));
        }

        public static int GetWeekNo(DateTime dt)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            System.Globalization.Calendar cal = dfi.Calendar;
            return cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }
        public static string AnyDateTimeToYearAndMonthAndDay(object dateTime)
        {
            DateTime dt = Convert.ToDateTime(dateTime);
            return string.Concat(dt.Year.ToString(), "-", dt.Month.ToString().PadLeft(2, '0'), "-", dt.Day.ToString().PadLeft(2, '0'));
        }


        //public static string DateTimeTotFileFormat(DateTime dt)
        //{
        //    string strDt = dt.ToString();
        //    strDt = ReplaceEx(strDt, ":", "_");

        //    return strDt;
        //}

        public static string MakeUrlFriendly(string strText)
        {
            string strClean = strText;
            strClean = strClean.Replace(" ", string.Empty);
            strClean = strClean.Replace('å', 'a');
            strClean = strClean.Replace('ä', 'a');
            strClean = strClean.Replace('ö', 'o');
            strClean = strClean.Replace('Å', 'A');
            strClean = strClean.Replace('Ä', 'A');
            strClean = strClean.Replace('Ö', 'O');
            strClean = strClean.Replace('é', 'e');
            return strClean;
        }

        public static string FriendlyDateTime(DateTime dt)
        {
            return string.Concat(dt.ToShortDateString(), " ", dt.ToShortTimeString());
        }





    }
}
