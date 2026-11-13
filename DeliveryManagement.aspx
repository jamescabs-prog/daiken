<%@ Page Title="Delivery Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DeliveryManagement.aspx.cs" Inherits="Drafters.DeliveryManagement" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-3">Delivery Management</h3>
    <div class="dlv-note">Administrative oversight of all deliveries. The delivery work itself (notices, receiving, discrepancies, verification, invoice and release) is done by the Delivery staff in their own Delivery area.</div>

    <div class="row g-3 mb-3">
        <div class="col-6 col-md-3"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">Awaiting Delivery</div><div class="num text-warning"><asp:Label ID="lblAwaiting" runat="server" /></div></div></div></div>
        <div class="col-6 col-md-3"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">Open Discrepancies (RepF)</div><div class="num text-danger"><asp:Label ID="lblOpenRep" runat="server" /></div></div></div></div>
        <div class="col-6 col-md-3"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">24h Inform Overdue</div><div class="num text-danger"><asp:Label ID="lblOverdue" runat="server" /></div></div></div></div>
        <div class="col-6 col-md-3"><div class="card dlv-stat"><div class="card-body"><div class="text-muted">Completed / All</div><div class="num text-success"><asp:Label ID="lblDone" runat="server" /></div></div></div></div>
    </div>

    <asp:Panel ID="pnlDetail" runat="server" Visible="false">
        <div class="card mb-4"><div class="card-body">
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
                <h5 class="mb-0">Delivery record &middot; <asp:Literal ID="litTitle" runat="server" /></h5>
                <a class="btn btn-sm btn-outline-primary" href="DeliveryManagement.aspx">Back to list</a>
            </div>
            <asp:Literal ID="litStepper" runat="server" />
            <asp:Literal ID="litSummary" runat="server" />
            <div class="dlv-section">Items</div><asp:Literal ID="litLines" runat="server" />
            <div class="dlv-section">Receiving history</div><asp:Literal ID="litReceipts" runat="server" />
            <div class="dlv-section">Discrepancy reports (RepF)</div><asp:Literal ID="litRep" runat="server" />
            <div class="dlv-section">Sales invoice &amp; delivery receipt</div><asp:Literal ID="litDocs" runat="server" />
            <div class="dlv-section">Activity log</div><asp:Literal ID="litLog" runat="server" />
        </div></div>
    </asp:Panel>

    <div class="card mb-3"><div class="card-body">
        <div class="row g-3 align-items-end">
            <div class="col-md-5"><label class="form-label">Search (delivery, PO, supplier, department, DR)</label><asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" /></div>
            <div class="col-md-4"><label class="form-label">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select" /></div>
            <div class="col-md-3 d-flex gap-2">
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-primary" OnClick="btnClear_Click" />
            </div>
        </div>
    </div></div>

    <div class="card mb-4"><div class="table-responsive">
        <asp:Repeater ID="rptRecords" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Delivery No.</th><th>PO No.</th><th>Supplier</th><th>Requesting dept.</th><th>Expected</th><th>DR No(s).</th><th>Status</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Supplier") %></td>
                    <td><%#: Eval("Client") %></td>
                    <td><%# Eval("ExpectedDate", "{0:MMM dd, yyyy}") %></td>
                    <td><%#: Eval("DrNos") %></td>
                    <td><%# DeliveryHtml.Badge(Eval("Status")) %></td>
                    <td><a class="btn btn-sm btn-outline-primary" href="DeliveryManagement.aspx?id=<%# Eval("Id") %>">View</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="No records found." Visible="false"></asp:Label>
    </div></div>

    <h5>Discrepancy reports (RepF)</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptReps" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>RepF No.</th><th>PO No.</th><th>Supplier</th><th>Issues</th><th>Created</th><th>24h inform</th><th>Status</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Supplier") %></td>
                    <td><%#: Eval("IssueSummary") %></td>
                    <td><%# Eval("Created", "{0:MMM dd, h:mm tt}") %></td>
                    <td><%# DeliveryHtml.InformBadge((DiscrepancyReport)Container.DataItem) %></td>
                    <td><%# DeliveryHtml.Badge(Eval("Status")) %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblNoReps" runat="server" CssClass="d-block text-center text-muted py-4" Text="No discrepancy reports." Visible="false"></asp:Label>
    </div></div>
</div>
</asp:Content>
