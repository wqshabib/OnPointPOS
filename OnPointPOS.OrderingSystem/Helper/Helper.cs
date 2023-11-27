using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using System.Web.UI.HtmlControls;
using System.Xml;

namespace ML.Site
{
    public class Helper
   { }
    //    public enum StyleTag
    //    {
    //        Width = 1
    //    }

    //    public static string ExtractTag(string strXml, string strTag)
    //    {
    //        return ExtractTag(strXml, strTag, false);
    //    }

    //    public static string ExtractTag(string strXml, string strTag, bool bInnerXml)
    //    {
    //        return ExtractTag(strXml, strTag, bInnerXml, false);
    //    }

    //    public static string ExtractTag(string strXml, string strTag, bool bInnerXml, bool bAddXmlTags)
    //    {
    //        if (strXml == string.Empty || strTag == string.Empty)
    //        {
    //            return string.Empty;
    //        }

    //        if (bAddXmlTags)
    //        {
    //            strXml = string.Format("<root>{0}</root>", strXml);
    //        }

    //        XmlDocument xml = new XmlDocument();

    //        try
    //        {
    //            xml.LoadXml(strXml);

    //            if (xml.SelectSingleNode(strTag) == null)
    //            {
    //                if (xml.ChildNodes[0].SelectSingleNode(strTag) != null)
    //                {
    //                    if (!bInnerXml)
    //                    {
    //                        return xml.ChildNodes[0].SelectSingleNode(strTag).InnerText;
    //                    }
    //                    else
    //                    {
    //                        return xml.ChildNodes[0].SelectSingleNode(strTag).InnerXml;
    //                    }
    //                }
    //                else
    //                {
    //                    return string.Empty;
    //                }
    //            }

    //            if (!bInnerXml)
    //            {
    //                return xml.SelectSingleNode(strTag).InnerText;
    //            }
    //            else
    //            {
    //                return xml.SelectSingleNode(strTag).InnerXml;
    //            }
    //        }
    //        catch { }
    //        { 
    //            return string.Empty;
    //        }
    //    }

    //    public static bool MetaTagExists(string strText, string strMetaTag)
    //    {
    //        if (strText.IndexOf(string.Format("[{0}]", strMetaTag)) > -1)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    public static bool MetaTagExists(string strText, string strMetaTag, bool bIgnoreBrackets)
    //    {
    //        if (strText.IndexOf(string.Format("{0}", strMetaTag)) > -1)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    public static string ReplaceMetaTag(string strText, string strMetaTag, string strNewText)
    //    {
    //        //return strText.Replace(string.Format("[{0}]", strMetaTag), strNewText);
    //        return ML.Common.Text.ReplaceEx(strText, string.Format("[{0}]", strMetaTag.Replace("[", string.Empty).Replace("]", string.Empty)), strNewText);
    //    }

    //    public static List<string> ExtractMetaTagParameters(string strMetaTag)
    //    {
    //        List<string> lstMetaTag = strMetaTag.Replace("[", string.Empty).Replace("]", string.Empty).Split(Convert.ToChar(";")).ToList();
    //        lstMetaTag.RemoveAt(0);
    //        return lstMetaTag;
    //    }

    //    public static string CreateLink(int intDigits)
    //    {
    //        const string LINK_CHARACTERS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    //        Random rnd = new Random();

    //        ML.Site.Link.tLinkDataTable dtLink;
    //        StringBuilder sb = new StringBuilder();

    //        bool bFound = true;

    //        while (bFound)
    //        {
    //            sb = new StringBuilder();
    //            for (int i = 0; i < intDigits; i++)
    //            {
    //                int intCharacter = rnd.Next(0, LINK_CHARACTERS.Length);
    //                sb.Append(LINK_CHARACTERS.Substring(intCharacter, 1));
    //            }
    //            dtLink = new ML.Site.LinkTableAdapters.tLinkTableAdapter().GetByLink(sb.ToString());
    //            if (dtLink.Rows.Count == 0)
    //            {
    //                bFound = false;
    //            }
    //        }

    //        return sb.ToString();
    //    }

    //    public static string RemoveHtml(string strHtml)
    //    {
    //        while(strHtml.IndexOf("<") >= 0)
    //        {
    //            int intStart = strHtml.IndexOf("<");
    //            int intStop = strHtml.IndexOf(">");

    //            strHtml = strHtml.Remove(intStart, intStop - intStart + 1);
    //        }

    //        return strHtml;
    //    }

    //    public static string BuildPushReceiptSms(string strPushMessage, string strStartLink, string strFirstName, string strLastName, string strReceipt)
    //    {
    //        string strMoblink = string.Format(ML.Common.Constants.MOBLINK_LINK_URL, strStartLink);

    //        string strSms = ML.Common.Text.ReplaceEx(strPushMessage, "[LINK]", strMoblink);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[RECEIPT]", strReceipt);
    //        strSms = BuildSms(strSms, strFirstName, strLastName);

    //        return strSms;
    //    }

    //    public static string BuildPushSms(string strPushMessage, string strStartLink, string strFirstName, string strLastName)
    //    {
    //        string strMoblink = string.Format(ML.Common.Constants.MOBLINK_LINK_URL, strStartLink);

    //        string strSms = ML.Common.Text.ReplaceEx(strPushMessage, "[LINK]", strMoblink);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[RECEIPT]", string.Empty); // Reset
    //        strSms = BuildSms(strSms, strFirstName, strLastName);

    //        return strSms;
    //    }

    //    public static string BuildSms(string strMessage, string strFirstName, string strLastName)
    //    {
    //        string strSms = strMessage;
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "\r\n", "\r\n\r\n");
    //        strSms = TryInjectNames(strSms, strFirstName, strLastName);
    //        strSms = TryInjectDateTime(strSms);

    //        return strSms;
    //    }

    //    public static string BuildTipAFriendSms(string strMessage, string strSenderFirstName, string strSenderLastName, string strSenderPhoneNo, string strFirstName, string strLastName)
    //    {
    //        string strSms = strMessage;

    //        strSms = ML.Common.Text.ReplaceEx(strMessage, "\r\n", "\r\n\r\n");
    //        strSms = TryInjectSender(strSms, strSenderFirstName, strSenderLastName, strSenderPhoneNo);
    //        strSms = TryInjectNames(strSms, strFirstName, strLastName);

    //        return strSms;
    //    }

    //    public static string BuildBookingSms(string strMessage, string strRootUrl, string strLink, string strFirstName, string strLastName, string strSender, string strStartDate, string strStartTime, string strEndDate, string strEndTime, string strOldStartDate, string strOldStartTime, string strLocation)
    //    {
    //        if(string.IsNullOrEmpty(strRootUrl))
    //        {
    //            strRootUrl = ML.Common.Constants.MOBLINK_LINK_URL;
    //        }
    //        string strMoblink = string.Format(strRootUrl, strLink);

    //        string strSms = ML.Common.Text.ReplaceEx(strMessage, "[LINK]", strMoblink);
    //        strSms = BuildSms(strSms, strFirstName, strLastName);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[SENDER]", strSender);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[STARTDATE]", strStartDate);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[STARTTIME]", strStartTime);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[ENDDATE]", strEndDate);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[ENDTIME]", strEndTime);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[OLDSTARTDATE]", strOldStartDate);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[OLDSTARTTIME]", strOldStartTime);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[LOCATION]", strLocation);
    //        return strSms;
    //    }

    //    //public static string BuildBookingChangeSms(string strBookingChangeMessage, string strBookingUpdateLink, string strFirstName, string strLastName)
    //    //{
    //    //    string strMoblink = string.Format(ML.Common.Constants.MOBLINK_LINK_URL, strBookingUpdateLink);

    //    //    string strSms = ML.Common.Text.ReplaceEx(strBookingChangeMessage, "[LINK]", strMoblink);
    //    //    strSms = BuildSms(strSms, strFirstName, strLastName);

    //    //    return strSms;
    //    //}

    //    //public static string BuildBookingCancelSms(string strBookingCancelMessage, string strBookingCancelLink, string strFirstName, string strLastName)
    //    //{
    //    //    string strMoblink = string.Format(ML.Common.Constants.MOBLINK_LINK_URL, strBookingCancelLink);

    //    //    string strSms = ML.Common.Text.ReplaceEx(strBookingCancelMessage, "[LINK]", strMoblink);
    //    //    strSms = BuildSms(strSms, strFirstName, strLastName);

    //    //    return strSms;
    //    //}

    //    private static string TryInjectNames(string strMessage, string strFirstName, string strLastName)
    //    {
    //        string strSms = strMessage;
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[FIRSTNAME]", strFirstName);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[LASTNAME]", strLastName);

    //        return strSms;
    //    }

    //    private static string TryInjectDateTime(string strMessage)
    //    {
    //        string strSms = strMessage;
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[DATE]", string.Format("{0:yyyy-MM-dd}", DateTime.Now));
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[TIME]", string.Format("{0:HH:mm:ss}", DateTime.Now));

    //        if (strSms.IndexOf("[DATE+") > -1)
    //        {
    //            int intStart = strSms.IndexOf("[DATE+") + 6;
    //            int intEnd = strSms.IndexOf("]", intStart);
    //            string strDays = strSms.Substring(intStart, intEnd - intStart);
    //            strSms = ML.Common.Text.ReplaceEx(strSms, string.Format("[DATE+{0}]", strDays), string.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(Convert.ToDouble(strDays))));
    //        }
    //        if (strSms.IndexOf("[TIME+") > -1)
    //        {
    //            int intStart = strSms.IndexOf("[TIME+") + 6;
    //            int intEnd = strSms.IndexOf("]", intStart);
    //            string strMinutes = strSms.Substring(intStart, intEnd - intStart);
    //            strSms = ML.Common.Text.ReplaceEx(strSms, string.Format("[TIME+{0}]", strMinutes), string.Format("{0:HH:mm:ss}", DateTime.Now.AddMinutes(Convert.ToDouble(strMinutes))));
    //        }
    //        //strText = string.Format("{0:yyyy-MM-dd HH:mm}", dtCustomerContentExt[0].CustomerContentTimeStamp);

    //        return strSms;
    //    }

    //    private static string TryInjectSender(string strMessage, string strSenderFirstName, string strSenderLastName, string strSenderPhoneNo)
    //    {
    //        string strSms = strMessage;
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[SENDERFIRSTNAME]", strSenderFirstName);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[SENDERLASTNAME]", strSenderLastName);
    //        strSms = ML.Common.Text.ReplaceEx(strSms, "[SENDERPHONENO]", strSenderPhoneNo);

    //        return strSms;
    //    }

    //    public static string ExtractStyle(string strStyle, StyleTag styleTag)
    //    {
    //        if (strStyle == string.Empty)
    //        {
    //            return string.Empty;
    //        }

    //        if (styleTag == StyleTag.Width)
    //        {
    //            int intValueStartPos = strStyle.ToLower().IndexOf("width:") + 6;
    //            if (intValueStartPos < 6)
    //            {
    //                return string.Empty;
    //            }
    //            int intValueEndPos = strStyle.ToLower().IndexOf(";", intValueStartPos);
    //            string strValue = strStyle.Substring(intValueStartPos, intValueEndPos - intValueStartPos);
    //            return strValue;
    //        }
    //        else
    //        {
    //            return string.Empty;
    //        }
    //    }

    //    public static string ReplaceStyle(string strStyle, StyleTag styleTag, string strReplace)
    //    {
    //        if (styleTag == StyleTag.Width)
    //        {
    //            int intValueStartPos = strStyle.ToLower().IndexOf("width:");
    //            if (intValueStartPos < 1)
    //            {
    //                return strStyle;
    //            }
    //            int intValueEndPos = strStyle.ToLower().IndexOf(";", intValueStartPos);
    //            string strOld = strStyle.Substring(intValueStartPos, intValueEndPos - intValueStartPos + 1);
    //            string strNew = ML.Common.Text.ReplaceEx(strStyle, strOld, string.Format("width:{0};", strReplace.Trim()));
    //            return strNew;
    //        }
    //        else
    //        {
    //            return strStyle;
    //        }
    //    }

    //    public static void UpdateColspans(Guid guidPageGuid)
    //    {
    //        // Update colspan for all td:s on page
    //        Dictionary<Guid, int> dic = new Dictionary<Guid, int>();
    //        ML.Site.Object.tObjectDataTable dtObject = new ML.Site.ObjectTableAdapters.tObjectTableAdapter().GetByPageGuidAndObjectTypeID(guidPageGuid, ML.Site.Constants.OBJECT_TYPE_TABLE_CELL);
    //        for (int i = 0; i < dtObject.Rows.Count; i++)
    //        {
    //            if (dic.ContainsKey(dtObject[i].ParentObjectGuid))
    //            {
    //                int intNoOf = dic[dtObject[i].ParentObjectGuid];
    //                dic[dtObject[i].ParentObjectGuid] = intNoOf + 1;
    //            }
    //            else
    //            {
    //                dic.Add(dtObject[i].ParentObjectGuid, 1);
    //            }
    //        }

    //        int intMode = 0;
    //        if (dic.ContainsValue(1))
    //        {
    //            intMode += 1;
    //        }
    //        if (dic.ContainsValue(2))
    //        {
    //            intMode += 2;
    //        }
    //        if (dic.ContainsValue(3))
    //        {
    //            intMode += 4;
    //        }
    //        if (dic.ContainsValue(4))
    //        {
    //            intMode += 8;
    //        }

    //        // Itterate and set new colspan
    //        for (int i = 0; i < dtObject.Rows.Count; i++)
    //        {
    //            XmlDocument xml = new XmlDocument();
    //            xml.LoadXml(dtObject[i].Xml);

    //            if (xml.SelectSingleNode("td") != null)
    //            {
    //                XmlAttribute xmlAttribute;
    //                if (xml.SelectSingleNode("td").Attributes["colspan"] != null)
    //                {
    //                    xmlAttribute = xml.SelectSingleNode("td").Attributes["colspan"];
    //                    xml.SelectSingleNode("td").Attributes.Remove(xmlAttribute);
    //                }

    //                foreach (KeyValuePair<Guid, int> kvp in dic)
    //                {
    //                    if (kvp.Key == dtObject[i].ParentObjectGuid)
    //                    {
    //                        xmlAttribute = xml.CreateAttribute("colspan");

    //                        if (intMode == 1)
    //                        {
    //                            // No need for colspan
    //                        }
    //                        else if (intMode == 2)
    //                        {
    //                            // No need for colspan
    //                        }
    //                        else if (intMode == 3)
    //                        {
    //                            if (kvp.Value == 1)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                        }
    //                        else if (intMode == 4)
    //                        {
    //                            // No need for colspan
    //                        }
    //                        else if (intMode == 5)
    //                        {
    //                            if (kvp.Value == 1)
    //                            {
    //                                xmlAttribute.Value = "3";
    //                            }
    //                        }
    //                        else if (intMode == 6)
    //                        {
    //                            if (kvp.Value == 2)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                            else if (kvp.Value == 3)
    //                            {
    //                                if (dtObject[i].PositionX == 1)
    //                                {
    //                                    xmlAttribute.Value = "2";
    //                                }
    //                            }
    //                        }
    //                        else if (intMode == 7)
    //                        {
    //                            if (kvp.Value == 1)
    //                            {
    //                                xmlAttribute.Value = "4";
    //                            }
    //                            else if (kvp.Value == 2)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                            else if (kvp.Value == 3)
    //                            {
    //                                if (dtObject[i].PositionX == 1)
    //                                {
    //                                    xmlAttribute.Value = "2";
    //                                }
    //                            }
    //                        }
    //                        else if (intMode == 8)
    //                        {
    //                            // No need for colspan
    //                        }
    //                        else if (intMode == 9)
    //                        {
    //                            if (kvp.Value == 1)
    //                            {
    //                                xmlAttribute.Value = "4";
    //                            }
    //                        }
    //                        else if (intMode == 10)
    //                        {
    //                            if (kvp.Value == 2)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                        }
    //                        else if (intMode == 11)
    //                        {
    //                            if (kvp.Value == 1)
    //                            {
    //                                xmlAttribute.Value = "4";
    //                            }
    //                            else if (kvp.Value == 2)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                        }
    //                        else if (intMode == 12)
    //                        {
    //                            if (kvp.Value == 3)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                            else if (kvp.Value == 4)
    //                            {
    //                                if (dtObject[i].PositionX == 1 || dtObject[i].PositionX == 2)
    //                                {
    //                                    xmlAttribute.Value = "2";
    //                                }
    //                            }
    //                        }
    //                        else if (intMode == 13)
    //                        {
    //                            if (kvp.Value == 1)
    //                            {
    //                                xmlAttribute.Value = "6";
    //                            }
    //                            else if (kvp.Value == 3)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                            else if (kvp.Value == 4)
    //                            {
    //                                if (dtObject[i].PositionX == 1 || dtObject[i].PositionX == 2)
    //                                {
    //                                    xmlAttribute.Value = "2";
    //                                }
    //                            }
    //                        }
    //                        else if (intMode == 14)
    //                        {
    //                            if (kvp.Value == 2)
    //                            {
    //                                xmlAttribute.Value = "3";
    //                            }
    //                            else if (kvp.Value == 3)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                            else if (kvp.Value == 4)
    //                            {
    //                                if (dtObject[i].PositionX == 1 || dtObject[i].PositionX == 2)
    //                                {
    //                                    xmlAttribute.Value = "2";
    //                                }
    //                            }
    //                        }
    //                        else if (intMode == 15)
    //                        {
    //                            if (kvp.Value == 1)
    //                            {
    //                                xmlAttribute.Value = "6";
    //                            }
    //                            else if (kvp.Value == 2)
    //                            {
    //                                xmlAttribute.Value = "3";
    //                            }
    //                            else if (kvp.Value == 3)
    //                            {
    //                                xmlAttribute.Value = "2";
    //                            }
    //                            else if (kvp.Value == 4)
    //                            {
    //                                if (dtObject[i].PositionX == 1 || dtObject[i].PositionX == 2)
    //                                {
    //                                    xmlAttribute.Value = "2";
    //                                }
    //                            }
    //                        }

    //                        if (xmlAttribute.Value != string.Empty)
    //                        {
    //                            xml.SelectSingleNode("td").Attributes.Append(xmlAttribute);
    //                        }
    //                    }
    //                }

    //                // Update xml
    //                new ML.Site.ObjectTableAdapters.tObjectTableAdapter().UpdateXml(xml.InnerXml, dtObject[i].ObjectGuid);
    //            }
    //        }
    //    }

    //    public static string TruncateText(ML.Site.Object.tObjectRow rowObject, string strText)
    //    {
    //        if (ML.Site.Helper.ExtractTag(rowObject.Xml, "maxcharacters") != "-1")
    //        {
    //            string strMaxCharacters = ML.Site.Helper.ExtractTag(rowObject.Xml, "maxcharacters");
    //            if (ML.Common.Text.IsNumeric(strMaxCharacters))
    //            {
    //                if (strText.Length > Convert.ToInt32(strMaxCharacters))
    //                {
    //                    int intLastSpacePos = strText.Substring(0, Convert.ToInt32(strMaxCharacters)).LastIndexOf(",");
    //                    if (intLastSpacePos == -1)
    //                    {
    //                        intLastSpacePos = strText.Substring(0, Convert.ToInt32(strMaxCharacters)).LastIndexOf(" ");
    //                    }
    //                    if (intLastSpacePos != -1)
    //                    {
    //                        strText = string.Format("{0}...", strText.Substring(0, Convert.ToInt32(intLastSpacePos)));
    //                    }
    //                    else
    //                    {
    //                        strText = string.Format("{0}...", strText.Substring(0, Convert.ToInt32(strMaxCharacters)));
    //                    }
    //                }
    //            }
    //        }
    //        return strText;
    //    }

    //    public static string ReplaceTAG(string strOrignalText, string strNewText)
    //    {
    //        if(strNewText.IndexOf("[TAG]") > -1)
    //        {
    //            // Clean tag
    //            strNewText = strOrignalText.Replace("[TAG]", string.Empty);
    //        }
    //        else if (strOrignalText.IndexOf("[TAG]") > -1)
    //        {
    //            // Replace tag
    //            strNewText = strOrignalText.Replace("[TAG]", strNewText);
    //        }
    //        return strNewText;
    //    }

    //    public static string ConvertUrlsToLinks(string strText)
    //    {
    //        if (strText.IndexOf("http://www.") > -1 || strText.IndexOf("http://www.") > -1 || strText.IndexOf("www.") > -1)
    //        {
    //            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
    //            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    //            return r.Replace(strText, "<a href=\"$1\" title=\"Klicka för att öppna i nytt fönster\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
    //        }
    //        else
    //        {
    //            return strText;
    //        }
    //    }

    //    public static string ReScaleFonts(string strText, double dblDevicePixelRatio, string strDeviceID)
    //    {
    //        //if (strDeviceID.IndexOf("iphone") > -1)
    //        //{
    //        //    return strText;
    //        //}

    //        string strNewText = string.Empty;

    //        int intStart = strText.ToLower().IndexOf("font-size:");
    //        if (intStart > -1)
    //        {
    //            int intEnd = strText.ToLower().IndexOf(";", intStart);
    //            string strKey = strText.Substring(intStart, intEnd - intStart);
    //            string strOldValue = System.Text.RegularExpressions.Regex.Replace(strKey, @"[^0-9.,]", string.Empty);
    //            if (ML.Common.Text.IsNumeric(strOldValue))
    //            {
    //                double dblNew = Convert.ToDouble(strOldValue.Replace(".", ",")) * dblDevicePixelRatio;
    //                string strNewKey = strKey.Replace(strOldValue, Math.Round(dblNew, 2).ToString()).Replace(",", ".");
    //                strNewText = strText.Replace(strKey, strNewKey);
    //            }
    //        }
    //        else
    //        {
    //            strNewText = string.Format("{0}font-size:{1}em;", strText, dblDevicePixelRatio.ToString().Replace(",", "."));
    //        }

    //        return strNewText;
    //    }

    //    public static StartLink CreateLinks(Guid guidSiteGuid, Guid guidCustomerGuid)
    //    {
    //        ML.Site.Site.tSiteDataTable dtSite = new ML.Site.SiteTableAdapters.tSiteTableAdapter().GetBySiteGuid(guidSiteGuid);
    //        if (dtSite.Rows.Count > 0)
    //        {
    //            return CreateLinks(dtSite[0], guidCustomerGuid);
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }

    //    public static StartLink CreateLinks(ML.Site.Site.tSiteRow site, Guid guidCustomerGuid)
    //    {
    //        // Handle old links
    //        ML.Site.Link.tLinkDataTable dtLink = new ML.Site.Link.tLinkDataTable();
    //        if (site.Permanent)
    //        {
    //            // Get old links
    //            dtLink = new ML.Site.LinkTableAdapters.tLinkTableAdapter().GetByCustomerGuid(guidCustomerGuid);
    //        }
    //        else
    //        {
    //            //Delete old Links
    //            new ML.Site.LinkTableAdapters.tLinkTableAdapter().DeleteLinks(
    //                site.SiteGuid
    //                , guidCustomerGuid
    //                );
    //        }

    //        // Create personal links if not existing
    //        ML.Site.Page.tPageDataTable dtPage = new ML.Site.PageTableAdapters.tPageTableAdapter().GetBySiteGuid(site.SiteGuid);
    //        string strStartLink = string.Empty;
    //        for (int j = 0; j < dtPage.Rows.Count; j++)
    //        {
    //            bool bLinkExists = false;

    //            for (int k = 0; k < dtLink.Rows.Count; k++)
    //            {
    //                if (dtPage[j].PageGuid == dtLink[k].PageGuid)
    //                {
    //                    if (dtPage[j].PageGuid == site.StartPageGuid)
    //                    {
    //                        strStartLink = dtLink[k].Link;
    //                    }
    //                    bLinkExists = true;
    //                    break;
    //                }
    //            }
    //            if (!bLinkExists)
    //            {
    //                string strLink = ML.Site.Helper.CreateLink(10);
    //                new ML.Site.LinkTableAdapters.tLinkTableAdapter().InsertLink(
    //                    strLink
    //                    , site.CompanyGuid
    //                    , site.SiteGuid
    //                    , dtPage[j].PageGuid
    //                    , guidCustomerGuid
    //                    );

    //                if (dtPage[j].PageGuid == site.StartPageGuid)
    //                {
    //                    strStartLink = strLink;
    //                }
    //            }
    //        }

    //        StartLink startLink = new StartLink();
    //        startLink.Link = strStartLink;
    //        startLink.PageGuid = site.StartPageGuid;

    //        return startLink;
    //    }

    //    public class StartLink
    //    {
    //        private string _strLink = string.Empty;
    //        private Guid _guidPageGuid = Guid.Empty;

    //        public string Link
    //        {
    //            set { this._strLink = value; }
    //            get { return this._strLink; }
    //        }

    //        public Guid PageGuid
    //        {
    //            set { this._guidPageGuid = value; }
    //            get { return this._guidPageGuid; }
    //        }
    //    }
    //}



}
