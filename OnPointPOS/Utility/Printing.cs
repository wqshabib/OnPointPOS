using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using POSSUM.Res;

namespace POSSUM
{
    public class Printing
    {
        private Font h1Font = new Font("Anonymous Pro", 14, FontStyle.Bold);
        private Font h2Font = new Font("Anonymous Pro", 12, FontStyle.Bold);
        private Font paragraphBoldFont = new Font("Anonymous Pro", 10, FontStyle.Bold);
        private Font paragraphFont = new Font("Anonymous Pro", 10, FontStyle.Bold);
        private PrintDocument pdoc;
        private string printData = "This is the default\rText\r\r\r\n\n\n";
        private string printerName = "SAM4S GIANT-100";

        public Printing()
        {
        }

        public Printing(string printData)
        {
            this.printData = printData;
        }

        public void setPrintData(string printData)
        {
            this.printData = printData;
        }

        public void setPrinterName(string printerName)
        {
            this.printerName = printerName;
        }

        public void setParagraphFont(Font paragraphFont)
        {
            this.paragraphFont = paragraphFont;
        }

        public void setParagraphBoldFont(Font paragraphBoldFont)
        {
            this.paragraphBoldFont = paragraphBoldFont;
        }

        public void setH1Font(Font h1Font)
        {
            this.h1Font = h1Font;
        }

        public void print(string printData)
        {
            this.printData = printData;
            new DirectPrint().PrintData(this.printData);

            //print();
        }

        public void print()
        {
            VerifyPrinterName();
            pdoc = new PrintDocument();
            PrinterSettings ps = new PrinterSettings();
            ps.PrinterName = printerName;

            pdoc.PrinterSettings = ps;

            PaperSize psize = pdoc.PrinterSettings.PaperSizes[1];

            pdoc.DefaultPageSettings.PaperSize = psize;

            Margins margins = new Margins(0, 0, 0, 0);
            pdoc.DefaultPageSettings.Margins = margins;

            pdoc.PrintPage += pdoc_PrintPage;

            pdoc.Print();
        }

        private void VerifyPrinterName()
        {
            PrintQueueCollection printers = new PrintServer().GetPrintQueues();
            PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
            var data = printers.Where(pn => pn.Name == printerName);
            if (data.Count() > 0)
            {
                prntque = printers.Single(pn => pn.Name == printerName);
            }
            else // Fall back to XPS Document Writer
            {
                printerName = "Microsoft XPS Document Writer";
            }
        }

        private void pdoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;

            int fontHeight = Convert.ToInt32(paragraphFont.GetHeight());
            int h1fontHeight = Convert.ToInt32(h1Font.GetHeight());
            int startX = 0;
            int startY = 0;
            int Offset = 0;
            //  Defaults.CompanyInfo.Name;
            if (File.Exists(Defaults.CompanyInfo.Logo))
            {
                Image image = Image.FromFile(Defaults.CompanyInfo.Logo);
                graphics.DrawImage(image, 0, 0);
                Offset = Offset + 50;
            }
            else
            {
                graphics.DrawString(Defaults.CompanyInfo.Name, h1Font, new SolidBrush(Color.Black), startX,
                    startY + Offset);
                Offset = Offset + h1fontHeight;
            }
            string orgNo = string.Format("{0,-10}{1,-26}", UI.Report_OrgNo, Defaults.CompanyInfo.OrgNo);
            graphics.DrawString(orgNo, paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + fontHeight;
            graphics.DrawString(" ", paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + fontHeight;
            string address = Defaults.CompanyInfo.Address;
            if (!string.IsNullOrEmpty(address))
            {
                if (Defaults.CompanyInfo.Address.Length > 26)
                {
                    string adrs = string.Format("{0,-10}{1,-26}", UI.Report_Address, address.Substring(0, 26));
                    graphics.DrawString(adrs, paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
                    Offset = Offset + fontHeight;
                    address = "";
                    int counter = 1;
                    foreach (var s in Defaults.CompanyInfo.Address.Skip(26))
                    {
                        if (counter % 26 == 0)
                        {
                            adrs = string.Format("{0,-10}{1,-26}", "", address);
                            graphics.DrawString(adrs, paragraphFont, new SolidBrush(Color.Black), startX,
                                startY + Offset);
                            Offset = Offset + fontHeight;
                            address = "";
                        }
                        address = address + s;
                        counter++;
                    }
                    if (!string.IsNullOrEmpty(address))
                    {
                        adrs = string.Format("{0,-10}{1,-26}", " ", address);
                        graphics.DrawString(adrs, paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
                        Offset = Offset + fontHeight;
                    }
                }
                else
                {
                    string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                    graphics.DrawString(addres, paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
                    Offset = Offset + fontHeight;
                }
            }
            graphics.DrawString(" ", paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + fontHeight;
            string phon = string.Format("{0,-10}{1,-26}", UI.Report_Phone, Defaults.CompanyInfo.Phone);
            graphics.DrawString(phon, paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + fontHeight;
            graphics.DrawString(" ", paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset = Offset + fontHeight;
            string[] lines = printData.Split((char) 13);
            foreach (string line in lines)
            {
                graphics.DrawString(line, paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
                Offset = Offset + fontHeight;
            }
        }
    }
}