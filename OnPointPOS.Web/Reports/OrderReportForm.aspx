<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OrderReportForm.aspx.cs" Inherits="POSSUM.Web.Reports.OrderReportForm" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="margin:0;">
    <style>

        .table-style {
            width:100% !important;
            height:100% !important;
        }

        table{
            overflow:hidden;
        }

        table#MyReportViewer_fixedTable
        {
            background:#fff;
            width:100% !important;
            height:100% !important;
        }
       
    </style>
    <form id="form1" runat="server" >
        <div class="panel-body br-t p12">
    <div style="background-color:white;width:100%;height:100%; overflow-x:auto; overflow-y:auto;" class="row"  >
        
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer CssClass="table-style" ID="MyReportViewer"  runat="server" SizeToReportContent="true" AsyncRendering="true" EnableEventValidation="false" BackColor="White" InteractiveHeight="0"></rsweb:ReportViewer>
   
    </div>
            </div>
    </form>
</body>
</html>
