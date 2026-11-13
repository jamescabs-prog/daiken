<%@ Page Title="Inventory Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="InventoryDashboard.aspx.cs" Inherits="Drafters.InventoryDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-4">Inventory Dashboard</h3>
    <div class="row g-3">
        <div class="col-md-3"><a class="text-decoration-none" href="Requests.aspx?status=Ready%20for%20Release">
            <div class="card"><div class="card-body"><div class="text-muted">Ready for Release</div><h2 class="text-info"><asp:Label ID="lblReady" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Requests.aspx?status=Pending%20-%20Awaiting%20Stock">
            <div class="card"><div class="card-body"><div class="text-muted">Awaiting Stock</div><h2 class="text-secondary"><asp:Label ID="lblAwaiting" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Inventory.aspx">
            <div class="card"><div class="card-body"><div class="text-muted">Low-stock items</div><h2 class="text-danger"><asp:Label ID="lblLow" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Replenishment.aspx">
            <div class="card"><div class="card-body"><div class="text-muted">Open RepF</div><h2><asp:Label ID="lblRepF" runat="server" /></h2></div></div></a></div>
    </div>

    <h5 class="mt-4">Approved requests for inventory</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptRelease" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>MRS No.</th><th>Requester</th><th>Status</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("MrsNo") %></b></td>
                    <td><%#: Eval("Requester.FullName") %></td>
                    <td><span class="badge bg-<%# Drafters.Services.Ui.Badge((string)Eval("Status")) %>"><%#: Eval("Status") %></span></td>
                    <td><a class="btn btn-sm btn-primary" href="RequestDetails.aspx?id=<%# Eval("RequisitionId") %>">Open</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblNoRelease" runat="server" CssClass="d-block text-center text-muted py-4" Text="Nothing to release." Visible="false"></asp:Label>
    </div></div>
</asp:Content>
