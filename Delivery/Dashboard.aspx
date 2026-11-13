<%@ Page Title="Delivery Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Drafters.Delivery.DashboardPage" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-4">Delivery Dashboard</h3>
    <div class="row g-3">
        <div class="col-6 col-md-3"><a class="text-decoration-none" href="Receiving.aspx"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">Awaiting Delivery</div><div class="num text-warning"><asp:Label ID="lblAwaiting" runat="server" /></div></div></div></a></div>
        <div class="col-6 col-md-3"><a class="text-decoration-none" href="Discrepancies.aspx"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">Open Discrepancies (RepF)</div><div class="num text-danger"><asp:Label ID="lblOpenRep" runat="server" /></div></div></div></a></div>
        <div class="col-6 col-md-3"><a class="text-decoration-none" href="Verification.aspx"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">For Verification</div><div class="num text-info"><asp:Label ID="lblVerify" runat="server" /></div></div></div></a></div>
        <div class="col-6 col-md-3"><a class="text-decoration-none" href="Release.aspx"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">For Invoice / Release</div><div class="num text-primary"><asp:Label ID="lblRelease" runat="server" /></div></div></div></a></div>
        <div class="col-6 col-md-3"><a class="text-decoration-none" href="DeliveryRecords.aspx"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">Completed</div><div class="num text-success"><asp:Label ID="lblDone" runat="server" /></div></div></div></a></div>
        <div class="col-6 col-md-3"><a class="text-decoration-none" href="Discrepancies.aspx"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">24h Inform Overdue</div><div class="num text-danger"><asp:Label ID="lblOverdue" runat="server" /></div></div></div></a></div>
        <div class="col-6 col-md-3"><a class="text-decoration-none" href="DeliveryRecords.aspx"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">All Deliveries</div><div class="num"><asp:Label ID="lblAll" runat="server" /></div></div></div></a></div>
    </div>

    <h5 class="mt-4">Deliveries waiting for action</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptOpen" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Delivery No.</th><th>PO No.</th><th>Supplier</th><th>Requesting dept.</th><th>Expected</th><th>Status</th><th>Next step</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Supplier") %></td>
                    <td><%#: Eval("Client") %></td>
                    <td><%# Eval("ExpectedDate", "{0:MMM dd, yyyy}") %></td>
                    <td><%# DeliveryHtml.Badge(Eval("Status")) %></td>
                    <td><a class="btn btn-sm btn-outline-primary" href='<%# NextUrl(Container.DataItem) %>'><%# NextLabel(Container.DataItem) %></a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblNone" runat="server" CssClass="d-block text-center text-muted py-4" Text="No deliveries in progress." Visible="false"></asp:Label>
    </div></div>

    <h5 class="mt-4">Delivery alerts</h5>
    <div class="card">
        <asp:Literal ID="litAlerts" runat="server" />
    </div>
</div>
</asp:Content>
