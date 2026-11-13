<%@ Page Title="Delivery Records" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DeliveryRecords.aspx.cs" Inherits="Drafters.Delivery.RecordsPage" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-3">Delivery Records</h3>

    <asp:Panel ID="pnlDetail" runat="server" Visible="false">
        <div class="card mb-4"><div class="card-body">
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
                <h5 class="mb-0">Record &middot; <asp:Literal ID="litTitle" runat="server" /></h5>
                <div class="d-flex gap-2">
                    <a class="btn btn-sm btn-outline-primary" href="DeliveryRecords.aspx">Back to list</a>
                    <asp:HyperLink ID="lnkNext" runat="server" CssClass="btn btn-sm btn-primary" />
                </div>
            </div>
            <asp:Literal ID="litStepper" runat="server" />
            <asp:Literal ID="litSummary" runat="server" />
            <div class="dlv-section">Items</div><asp:Literal ID="litLines" runat="server" />
            <div class="dlv-section">Receiving history</div><asp:Literal ID="litReceipts" runat="server" />
            <div class="dlv-section">Discrepancy reports</div><asp:Literal ID="litRep" runat="server" />
            <div class="dlv-section">Sales invoice &amp; delivery receipt</div><asp:Literal ID="litDocs" runat="server" />
            <div class="dlv-section">Activity log</div><asp:Literal ID="litLog" runat="server" />
        </div></div>
    </asp:Panel>

    <div class="card mb-3"><div class="card-body">
        <div class="row g-3 align-items-end">
            <div class="col-md-5"><label class="form-label">Search (delivery, PO, supplier, DR)</label><asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" /></div>
            <div class="col-md-4"><label class="form-label">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select" /></div>
            <div class="col-md-3 d-flex gap-2">
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-primary" OnClick="btnClear_Click" />
            </div>
        </div>
    </div></div>

    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptRecords" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Delivery No.</th><th>PO No.</th><th>Supplier</th><th>Requesting dept.</th><th>DR No(s).</th><th>PO status</th><th>Status</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Supplier") %></td>
                    <td><%#: Eval("Client") %></td>
                    <td><%#: Eval("DrNos") %></td>
                    <td><%#: Eval("PoStatus") %></td>
                    <td><%# DeliveryHtml.Badge(Eval("Status")) %></td>
                    <td><a class="btn btn-sm btn-outline-primary" href="DeliveryRecords.aspx?id=<%# Eval("Id") %>">View</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="No records found." Visible="false"></asp:Label>
    </div></div>
</div>
</asp:Content>
