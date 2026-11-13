<%@ Page Title="Discrepancies (RepF)" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Discrepancies.aspx.cs" Inherits="Drafters.Delivery.DiscrepanciesPage" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-3">Delivery Discrepancies (RepF)</h3>

    <asp:Panel ID="pnlDetail" runat="server" Visible="false">
        <div class="card mb-3"><div class="card-body">
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
                <h5 class="mb-0"><asp:Literal ID="litHead" runat="server" /></h5>
                <a class="btn btn-sm btn-outline-primary" href="Discrepancies.aspx">Back to list</a>
            </div>
            <asp:Literal ID="litStepper" runat="server" />
            <asp:Literal ID="litSummary" runat="server" />
            <div class="dlv-section">Missing, incorrect or damaged items</div>
            <asp:Literal ID="litIssues" runat="server" />
        </div></div>

        <div class="card mb-4"><div class="card-body">
            <div class="dlv-section">1 &middot; Inform the supplier and the client within 24 hours</div>
            <p><asp:Literal ID="litDeadline" runat="server" /></p>
            <div class="row g-3">
                <div class="col-md-6"><b>Supplier:</b> <asp:Literal ID="litInfS" runat="server" />
                    <div class="mt-2"><asp:Button ID="btnInfS" runat="server" Text="Mark supplier as informed" CssClass="btn btn-sm btn-primary" OnClick="btnInfS_Click" /></div></div>
                <div class="col-md-6"><b>Client / requesting dept.:</b> <asp:Literal ID="litInfC" runat="server" />
                    <div class="mt-2"><asp:Button ID="btnInfC" runat="server" Text="Mark client as informed" CssClass="btn btn-sm btn-primary" OnClick="btnInfC_Click" /></div></div>
            </div>

            <div class="dlv-section">2 &middot; Coordinate with supplier for replacement or return</div>
            <asp:Literal ID="litRes" runat="server" />
            <asp:Panel ID="pnlRes" runat="server">
                <div class="row g-3">
                    <div class="col-md-6"><label class="form-label">Resolution</label>
                        <asp:DropDownList ID="ddlRes" runat="server" CssClass="form-select"><asp:ListItem Value="Replacement">Replacement of items</asp:ListItem><asp:ListItem Value="Return">Return of items</asp:ListItem></asp:DropDownList></div>
                    <div class="col-md-6"><label class="form-label">Supplier deadline (replacement / correct items)</label>
                        <asp:TextBox ID="txtDeadline" runat="server" TextMode="Date" CssClass="form-control" /></div>
                </div>
                <div class="mt-3"><asp:Button ID="btnRes" runat="server" Text="Save coordination" CssClass="btn btn-sm btn-primary" OnClick="btnRes_Click" /></div>
            </asp:Panel>

            <div class="dlv-section">3 &middot; Follow up with the supplier (according to PO)</div>
            <asp:Literal ID="litFollow" runat="server" />
            <asp:Panel ID="pnlFollow" runat="server">
                <label class="form-label">Follow-up note</label>
                <asp:TextBox ID="txtNote" runat="server" CssClass="form-control" MaxLength="200" placeholder="e.g. Called supplier - replacement truck scheduled" />
                <div class="mt-3"><asp:Button ID="btnFollow" runat="server" Text="Add follow-up" CssClass="btn btn-sm btn-primary" OnClick="btnFollow_Click" /></div>
            </asp:Panel>

            <div class="dlv-section">4 &middot; Updated PO status</div>
            <p><asp:Literal ID="litPo" runat="server" /></p>
            <asp:HyperLink ID="lnkReceive" runat="server" CssClass="btn btn-success">Receive replacement / correct items</asp:HyperLink>
        </div></div>
    </asp:Panel>

    <h5>All discrepancy reports</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptReps" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>RepF No.</th><th>PO No.</th><th>Supplier</th><th>Issues</th><th>Created</th><th>24h inform</th><th>Status</th><th></th></tr></thead>
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
                    <td><a class="btn btn-sm btn-outline-primary" href="Discrepancies.aspx?r=<%# Eval("Id") %>">Open</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="No discrepancy reports." Visible="false"></asp:Label>
    </div></div>
</div>
</asp:Content>
