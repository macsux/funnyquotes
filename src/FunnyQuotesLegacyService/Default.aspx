<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FunnyQuotesLegacyService.Default"  %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Health Check</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
                 <a runat="server" href="~/FunnyQuoteserviceLegacy.asmx">ASMX service</a>
                 <a runat="server"  href="~/FunnyQuoteserviceWCF.svc">WCF service</a>   
        </div>
    </form>
</body>
</html>
