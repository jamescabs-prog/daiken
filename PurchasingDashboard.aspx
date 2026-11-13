<%@ Page Title="Purchasing Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PurchasingDashboard.aspx.cs" Inherits="Drafters.PurchasingDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-4">Purchasing Dashboard</h3>
    <div class="row g-3">
        <div class="col-md-3"><a class="text-decoration-none" href="Replenishment.aspx">
            <div class="card"><div class="card-body"><div class="text-muted">Pending RepF</div><h2 class="text-warning"><asp:Label ID="lblPendingRep" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Replenishment.aspx">
            <div class="card"><div class="card-body"><div class="text-muted">Units still short</div><h2 class="text-danger"><asp:Label ID="lblShort" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Replenishment.aspx">
            <div class="card"><div class="card-body"><div class="text-muted">Reminders received</div><h2 class="text-info"><asp:Label ID="lblReminders" runat="server" /></h2></div></div></a></div>
        <div class="col-md-3"><a class="text-decoration-none" href="Replenishment.aspx">
            <div class="card"><div class="card-body"><div class="text-muted">Received / Completed</div><h2 class="text-success"><asp:Label ID="lblReceived" runat="server" /></h2></div></div></a></div>
    </div>

    <div class="d-flex justify-content-between align-items-center mt-4 mb-2">
        <h5 class="mb-0">Replenishment requests waiting for stock</h5>
        <a class="btn btn-sm btn-primary" href="Replenishment.aspx">Open Replenishment (RepF)</a>
    </div>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptPending" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>RepF No.</th><th>For MRS</th><th>Item</th><th class="text-end">Still short</th><th class="text-end">Reminders</th><th>Created</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("RepNo") %></b></td>
                    <td><%#: Eval("Form.MrsNo") %></td>
                    <td><%#: Eval("Item.ItemName") %></td>
                    <td class="text-end"><%# Eval("QtyShort") %></td>
                    <td class="text-end"><%# Eval("ReminderCount") %></td>
                    <td><%# Eval("CreatedAt", "{0:MMM dd, h:mm tt}") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblNone" runat="server" CssClass="d-block text-center text-muted py-4" Text="No replenishment request is waiting." Visible="false"></asp:Label>
    </div></div>
</asp:Content>
