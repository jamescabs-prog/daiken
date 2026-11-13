<%@ Page Title="Issuance Slip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SlipPrint.aspx.cs" Inherits="Drafters.SlipPrint" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="no-print mb-3"><button type="button" class="btn btn-dark" onclick="window.print()">Print</button></div>
    <div class="card"><div class="card-body p-4">
        <h4 class="text-center"><asp:Label ID="lblHeading" runat="server" /></h4>
        <hr />
        <div class="row mb-3">
            <div class="col-6"><b>IS No.:</b> <asp:Label ID="lblIsNo" runat="server" /><br />
                <b>MRS No.:</b> <asp:Label ID="lblMrs" runat="server" /><br />
                <b>Job Order:</b> <asp:Label ID="lblJob" runat="server" /></div>
            <div class="col-6 text-end"><b>Date generated:</b> <asp:Label ID="lblDate" runat="server" /><br />
                <b>AR No.:</b> <asp:Label ID="lblArNo" runat="server" /></div>
        </div>
        <p><b>Requester:</b> <asp:Label ID="lblRequester" runat="server" /><br />
           <b>Purpose:</b> <asp:Label ID="lblPurpose" runat="server" /></p>
        <asp:Repeater ID="rptLines" runat="server">
            <HeaderTemplate>
                <table class="table table-bordered">
                    <thead><tr><th>Item Code</th><th>Description</th><th class="text-end">Qty</th><th>Unit</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr><td><%#: Eval("Item.ItemCode") %></td><td><%#: Eval("Item.ItemName") %></td><td class="text-end"><%# Eval("Qty") %></td><td><%#: Eval("Item.Unit") %></td></tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <div class="row mt-5 text-center">
            <div class="col-4"><hr />Released by<br /><b><asp:Label ID="lblReleasedBy" runat="server" /></b></div>
            <div class="col-4"><hr />Approved by<br /><b><asp:Label ID="lblApprovedBy" runat="server" /></b></div>
            <div class="col-4"><hr />Received by (signature)<br /><b><asp:Label ID="lblReceivedBy" runat="server" /></b></div>
        </div>
    </div></div>
</asp:Content>
