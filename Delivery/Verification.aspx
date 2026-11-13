<%@ Page Title="Verification" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Verification.aspx.cs" Inherits="Drafters.Delivery.VerificationPage" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-3">Delivery Verification</h3>

    <asp:Panel ID="pnlDetail" runat="server" Visible="false">
        <div class="card mb-3"><div class="card-body">
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
                <h5 class="mb-0">Verify records &middot; <asp:Literal ID="litTitle" runat="server" /></h5>
                <a class="btn btn-sm btn-outline-primary" href="Verification.aspx">Back to list</a>
            </div>
            <asp:Literal ID="litStepper" runat="server" />
            <asp:Literal ID="litSummary" runat="server" />
            <div class="dlv-section">PO quantities vs. accepted items vs. inventory records</div>
            <asp:Literal ID="litLines" runat="server" />
            <div class="dlv-note"><b>Outstanding items?</b> <asp:Literal ID="litOut" runat="server" /></div>
            <div class="dlv-section">Received under</div>
            <asp:Literal ID="litReceipts" runat="server" />
        </div></div>

        <asp:Panel ID="pnlAct" runat="server" CssClass="card mb-4">
            <div class="card-body">
                <div class="dlv-section">Verification checklist</div>
                <div class="form-check"><asp:CheckBox ID="chk1" runat="server" Text="The supplier DR is attached to the PO and the PO is marked as delivered" /></div>
                <div class="form-check"><asp:CheckBox ID="chk2" runat="server" Text="Accepted quantities match the PO and there are no outstanding items" /></div>
                <div class="form-check"><asp:CheckBox ID="chk3" runat="server" Text="Inventory records were updated and match the accepted quantities" /></div>
                <div class="mt-3"><label class="form-label">Remarks (optional)</label><asp:TextBox ID="txtNote" runat="server" CssClass="form-control" MaxLength="200" /></div>
                <div class="mt-3"><asp:Button ID="btnOk" runat="server" Text="Confirm &amp; proceed to invoicing" CssClass="btn btn-success" OnClick="btnOk_Click" /></div>
            </div>
        </asp:Panel>
    </asp:Panel>

    <asp:Literal ID="litRO" runat="server" />

    <h5>Waiting for verification</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptWaiting" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Delivery No.</th><th>PO No.</th><th>Supplier</th><th>Requesting dept.</th><th>DR No(s).</th><th class="text-end">Accepted qty</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Supplier") %></td>
                    <td><%#: Eval("Client") %></td>
                    <td><%#: Eval("DrNos") %></td>
                    <td class="text-end"><%# Eval("AcceptedTotal") %></td>
                    <td><a class="btn btn-sm btn-primary" href="Verification.aspx?id=<%# Eval("Id") %>">Verify</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="Nothing is waiting for verification." Visible="false"></asp:Label>
    </div></div>
</div>
</asp:Content>
