﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace POSSUM.Api.Utility
{
    public class Converter
    {
        public static HiQPdf.PdfDocument HtmlToPdf(string html)
        {
            HiQPdf.HtmlToPdf htmlToPdf = new HiQPdf.HtmlToPdf();
            htmlToPdf.SerialNumber = Constants.SERIAL;
            return htmlToPdf.ConvertHtmlToPdfDocument(html, null);
        }
        //public static MemoryStream HtmlToStream(string html)
        //{
        //    return HtmlToStream(html, Utility.GetPdfFooterString());
        //    public static MemoryStream HtmlToStream(string html, string footer)
        //}

        public static MemoryStream HtmlToStream(string html)
        {
            HiQPdf.HtmlToPdf htmlToPdf = new HiQPdf.HtmlToPdf();
            htmlToPdf.SerialNumber = Constants.SERIAL;

            //PdfPageSize.A4, PdfDocumentMargins.Empty, GetSelectedPageOrientation());

            // set PDF page size and orientation
            htmlToPdf.Document.PageSize = HiQPdf.PdfPageSize.A4; // GetSelectedPageSize();
            htmlToPdf.Document.PageOrientation = HiQPdf.PdfPageOrientation.Portrait;   // GetSelectedPageOrientation();

            // set PDF page margins
            //htmlToPdf.Document.Margins = new PdfMargins(5);
            htmlToPdf.Document.Margins = new HiQPdf.PdfMargins(50, 50, 50, 50);

            //            HiQPdf.PdfHtml headerHtml = new HiQPdf.PdfHtml(50, 5, @"<span style=""color:Navy; font-family:Times New Roman; font-style:italic"">
            //                    Quickly Create High Quality PDFs with </span><a href=""http://www.hiqpdf.com"">HiQPdf</a>", null);
            //            htmlToPdf.Document.Header.Enabled = true;
            //            htmlToPdf.Document.Header.Layout(headerHtml);

            // Footer
            //htmlToPdf.Document.Footer.Enabled = false;
            //HiQPdf.PdfHtml footerHtml = new HiQPdf.PdfHtml(footer, null);
            //htmlToPdf.Document.Footer.Layout(footerHtml);


            MemoryStream ms = new MemoryStream();
            htmlToPdf.ConvertHtmlToStream(html, null, ms);
            return ms;
        }
        public class Constants
        {
            public const string SERIAL = "7aWEvL2J-i6GEj5+M-n5TVw93N-3M3czdre-1M3e3MPc-38PU1NTU";

        }


    }
}