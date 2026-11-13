<%@ Page Title="Invoice and Release" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Release.aspx.cs" Inherits="Drafters.Delivery.ReleasePage" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-3">Invoice &amp; Release</h3>

    <asp:Panel ID="pnlDetail" runat="server" Visible="false">
        <div class="card mb-3"><div class="card-body">
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
                <h5 class="mb-0"><asp:Literal ID="litTitle" runat="server" /></h5>
                <a class="btn btn-sm btn-outline-primary" href="Release.aspx">Back to list</a>
            </div>
            <asp:Literal ID="litStepper" runat="server" />
            <asp:Literal ID="litSummary" runat="server" />
            <asp:Literal ID="litLines" runat="server" />
        </div></div>

        <asp:Panel ID="pnlPrepare" runat="server" CssClass="card mb-3">
            <div class="card-body">
                <h5>Prepare sales invoice and delivery receipts</h5>
                <p class="text-muted">The records were verified. Generate the sales invoice and delivery receipt numbers for this delivery.</p>
                <asp:Button ID="btnPrepare" runat="server" Text="Prepare sales invoice &amp; delivery receipt" CssClass="btn btn-primary" OnClick="btnPrepare_Click" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlRelease" runat="server" CssClass="card mb-4">
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <h5 class="mb-0">Documents</h5>
                    <a class="btn btn-sm btn-outline-primary no-print" href="#" onclick="window.print();return false;">Print</a>
                </div>
                <asp:Literal ID="litDocs" runat="server" />
                <asp:Panel ID="pnlReleaseForm" runat="server">
                    <div class="dlv-section">Release items to the requesting department / client</div>
                    <div class="row g-3">
                        <div class="col-md-4"><label class="form-label">Released to (department / client)</label><asp:TextBox ID="txtTo" runat="server" CssClass="form-control" MaxLength="80" /></div>
                        <div class="col-md-4"><label class="form-label">Received and signed by</label><asp:TextBox ID="txtSigned" runat="server" CssClass="form-control" MaxLength="60" placeholder="Name of the person who signed" /></div>
                        <div class="col-md-4"><label class="form-label">Signed receipt reference no.</label><asp:TextBox ID="txtRef" runat="server" CssClass="form-control" MaxLength="40" placeholder="e.g. SR-2026-0032" /></div>
                    </div>
                    <div class="mt-3"><asp:Button ID="btnRelease" runat="server" Text="Release items &amp; mark completed" CssClass="btn btn-success" OnClick="btnRelease_Click" /></div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </asp:Panel>

    <asp:Literal ID="litRO" runat="server" />

    <h5>Waiting for invoice / release</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptWaiting" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Delivery No.</th><th>PO No.</th><th>Requesting dept.</th><th class="text-end">Qty</th><th>Status</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Client") %></td>
                    <td class="text-end"><%# Eval("AcceptedTotal") %></td>
                    <td><%# DeliveryHtml.Badge(Eval("Status")) %></td>
                    <td><a class="btn btn-sm btn-primary" href="Release.aspx?id=<%# Eval("Id") %>"><%# DeliveryHtml.ActionLabel((DeliveryRecord)Container.DataItem) %></a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="Nothing is waiting for invoicing or release." Visible="false"></asp:Label>
    </div></div>
</div>
</asp:Content>
