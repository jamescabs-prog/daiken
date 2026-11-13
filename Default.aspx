<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Drafters.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-4">Admin Dashboard</h3>
    <div class="row g-3">
        <div class="col-md-3"><a class="text-decoration-none" href="Requests.aspx?status=Pending%20Approval">
            <div class="card"><div class="card-body"><div class="text-muted">Pending Approval</div><h2 class="text-warning"><asp:Label ID="lblPending" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Requests.aspx?status=Ready%20for%20Release">
            <div class="card"><div class="card-body"><div class="text-muted">Ready for Release</div><h2 class="text-info"><asp:Label ID="lblReady" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Requests.aspx?status=Pending%20-%20Awaiting%20Stock">
            <div class="card"><div class="card-body"><div class="text-muted">Awaiting Stock</div><h2 class="text-secondary"><asp:Label ID="lblAwaiting" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Requests.aspx?status=For%20Revision">
            <div class="card"><div class="card-body"><div class="text-muted">Returned for Revision</div><h2 class="text-danger"><asp:Label ID="lblRevision" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Requests.aspx?archived=1">
            <div class="card"><div class="card-body"><div class="text-muted">Completed / Archived</div><h2 class="text-success"><asp:Label ID="lblCompleted" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><div class="card"><div class="card-body"><div class="text-muted">Low-stock items</div><h2 class="text-danger"><asp:Label ID="lblLow" runat="server" /></h2></div></div></div>
        <div class="col-md-3"><div class="card"><div class="card-body"><div class="text-muted">Open RepF</div><h2><asp:Label ID="lblRepF" runat="server" /></h2></div></div></div>
    </div>

    <asp:Panel ID="pnlAdmin" runat="server">
        <h5 class="mt-4">Pending requests waiting for your review</h5>
        <div class="card"><div class="table-responsive">
            <asp:Repeater ID="rptPending" runat="server">
                <HeaderTemplate>
                    <table class="table table-hover mb-0 align-middle">
                        <thead class="table-light"><tr><th>MRS No.</th><th>Requester</th><th>Items requested</th><th>Stock</th><th>Submitted</th><th></th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><b><%#: Eval("MrsNo") %></b> <%# RevisionBadge(Container.DataItem) %></td>
                        <td><%#: Eval("Requester.FullName") %><br /><small class="text-muted"><%#: Eval("Department") %></small></td>
                        <td><%# ItemsHtml(Container.DataItem) %></td>
                        <td><%# StockBadge(Container.DataItem) %></td>
                        <td><%# Eval("CreatedAt", "{0:MMM dd, h:mm tt}") %></td>
                        <td><a class="btn btn-sm btn-primary" href="RequestDetails.aspx?id=<%# Eval("RequisitionId") %>">Review</a></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
            <asp:Label ID="lblNoPending" runat="server" CssClass="d-block text-center text-muted py-4" Text="No pending requests." Visible="false"></asp:Label>
        </div></div>
    </asp:Panel>

</asp:Content>
