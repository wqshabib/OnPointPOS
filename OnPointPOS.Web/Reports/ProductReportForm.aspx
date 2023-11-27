<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductReportForm.aspx.cs" Inherits="POSSUM.Web.Reports.ProductReportForm" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" style="overflow:hidden">
<head runat="server">
    <title></title>
</head>
<body style="margin:0;">
    <style>
       
        table#MyReportViewer_fixedTable{background:#fff;min-width:1200px;}
       
    </style>
    <form id="form1" runat="server" >
        <div class="panel-body br-t p12">
    <div style="background-color:white;width:100%;min-height:700px;overflow-x:auto; overflow-y:auto; max-height:700px;" class="row"  >
        
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="MyReportViewer"  runat="server" SizeToReportContent="true" AsyncRendering="true" EnableEventValidation="false" BackColor="White"></rsweb:ReportViewer>
   
    </div>
            </div>
    </form>
</body>
</html>

<script src="../Scripts/jquery-1.10.2.js"></script>
<script>
    $(function () {
        $('#btnPrint').click(function () {
            printReport('MyReportViewer_ctl09');
        });
        showPrintButton();
    });
    function printReport(report_ID) {

        var rv1 = $('#' + report_ID);
        var iDoc = rv1.parents('html');

        // Reading the report styles
        var styles = iDoc.find("head style[id$='ReportControl_styles']").html();
        if ((styles == undefined) || (styles == '')) {
            iDoc.find('head script').each(function () {
                var cnt = $(this).html();
                var p1 = cnt.indexOf('ReportStyles":"');
                if (p1 > 0) {
                    p1 += 15;
                    var p2 = cnt.indexOf('"', p1);
                    styles = cnt.substr(p1, p2 - p1);
                }
            });
        }
        if (styles == '') { alert("Cannot generate styles, Displaying without styles.."); }
        styles = '<style type="text/css">' + styles + "</style>";

        // Reading the report html
        var table = rv1.find("div[id$='_oReportDiv']");
        if (table == undefined) {
            alert("Report source not found.");
            return;
        }

        // Generating a copy of the report in a new window
        var docType = '<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/loose.dtd">';
        var docCnt = styles + table.parent().html();
        var docHead = '<head><title></title><style>body{margin:5;padding:0;}</style></head>';
        var winAttr = "location=yes, statusbar=no, directories=no, menubar=no, titlebar=no, toolbar=no, dependent=no, width=720, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";;
        var newWin = window.open("", "_blank", winAttr);
        writeDoc = newWin.document;
        writeDoc.open();
        writeDoc.write(docType + '<html><body onload="window.print();">' + docCnt + '</body></html>');
        writeDoc.close();

        // The print event will fire as soon as the window loads
        newWin.focus();
        // uncomment to autoclose the preview window when printing is confirmed or canceled.
        // newWin.close();
    };
 
    //this method cab be called on body load onload="showPrintButton()"
    function showPrintButton() {
        var table = $("table[title='Refresh']");
        var parentTable = $(table).parents('table');
        var parentDiv = $(parentTable).parents('div').parents('div').first();
        var btnPrint = $("<input type='button' id='btnPrint' name='btnPrint' value='Print' style=\"font-family:Verdana;font-size:8pt;width:86px\"/>");
        
        btnPrint.click(function () {
            printReport('MyReportViewer');
        });
      
        if (parentDiv.find("input[value='Print']").length == 0) {
            parentDiv.append('<table cellpadding="0" cellspacing="0" toolbarspacer="true" style="display:inline-block;width:6px;"><tbody><tr><td></td></tr></tbody></table>');
            parentDiv.append('<div id="customDiv" class=" " style="display:inline-block;font-family:Verdana;font-size:8pt;vertical-align:inherit;"><table cellpadding="0" cellspacing="0"><tbody><tr><td><span style="cursor:pointer;" class="HighlightDiv" onclick="javascript:PrintRDLC();" ><img src="../../Content/img/printer.gif" alt="Print Report" title="Print Report" width="18px" height="18px" style="margin-top:4px"/></span></td></tr></tbody></table></div>');
           
        }
    }
    function PrintRDLC() {
        printReport('MyReportViewer');
    }

</script>
