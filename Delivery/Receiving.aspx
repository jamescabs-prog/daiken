<%@ Page Title="Receiving" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Receiving.aspx.cs" Inherits="Drafters.Delivery.ReceivingPage" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-3">Receiving</h3>

    <asp:Panel ID="pnlReceive" runat="server" Visible="false">
        <div class="card mb-3"><div class="card-body">
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
                <h5 class="mb-0">Receive delivery &middot; <asp:Literal ID="litTitle" runat="server" /></h5>
                <a class="btn btn-sm btn-outline-primary" href="Receiving.aspx">Back to list</a>
            </div>
            <asp:Literal ID="litStepper" runat="server" />
            <asp:Literal ID="litSummary" runat="server" />
            <asp:Literal ID="litWarn" runat="server" />
        </div></div>

        <div class="card mb-4"><div class="card-body">
            <div class="dlv-section">1 &middot; Supplier delivery receipt</div>
            <div class="row g-3">
                <div class="col-md-6"><label class="form-label">Supplier DR number</label><asp:TextBox ID="txtDr" runat="server" CssClass="form-control" MaxLength="40" placeholder="e.g. DR-55102" /></div>
                <div class="col-md-6"><label class="form-label">Received by</label><asp:TextBox ID="txtBy" runat="server" CssClass="form-control" MaxLength="60" /></div>
            </div>

            <div class="dlv-section">2 &middot; Checklist</div>
            <div class="form-check"><asp:CheckBox ID="chk1" runat="server" Text="Supplier DR is presented and refers to the correct PO" /></div>
            <div class="form-check"><asp:CheckBox ID="chk2" runat="server" Text="Items delivered match the PO item list" /></div>
            <div class="form-check"><asp:CheckBox ID="chk3" runat="server" Text="Packaging and seals are intact (no visible damage)" /></div>
            <div class="form-check"><asp:CheckBox ID="chk4" runat="server" Text="Quantities have been counted against the PO" /></div>

            <div class="dlv-section">3 &middot; Check items against the PO</div>
            <div class="dlv-note">Accepted = delivered &minus; damaged &minus; incorrect. Anything missing, damaged or incorrect creates a <b>discrepancy report (RepF)</b>; the good items are still accepted as a <b>partial receipt</b> and inventory is updated.</div>
            <asp:Repeater ID="rptItems" runat="server">
                <HeaderTemplate>
                    <div class="table-responsive"><table class="table align-middle mb-0">
                        <thead class="table-light"><tr><th>Item</th><th class="text-end">Ordered</th><th class="text-end">Accepted so far</th><th class="text-end">Expected now</th><th class="text-end">Delivered</th><th class="text-end">Damaged</th><th class="text-end">Incorrect</th><th>Remarks</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><b><%#: Eval("Name") %></b><div class="dlv-sub"><%#: Eval("Sku") %></div><asp:HiddenField ID="hfSku" runat="server" Value='<%# Eval("Sku") %>' /><asp:HiddenField ID="hfName" runat="server" Value='<%# Eval("Name") %>' /></td>
                        <td class="text-end"><%# Eval("Ordered") %></td>
                        <td class="text-end"><%# Eval("Accepted") %></td>
                        <td class="text-end"><b><%# Eval("Outstanding") %></b></td>
                        <td><asp:TextBox ID="txtDel" runat="server" TextMode="Number" min="0" CssClass="form-control form-control-sm dlv-num ms-auto" Text='<%# Eval("Outstanding") %>' /></td>
                        <td><asp:TextBox ID="txtDmg" runat="server" TextMode="Number" min="0" CssClass="form-control form-control-sm dlv-num ms-auto" Text="0" /></td>
                        <td><asp:TextBox ID="txtInc" runat="server" TextMode="Number" min="0" CssClass="form-control form-control-sm dlv-num ms-auto" Text="0" /></td>
                        <td><asp:TextBox ID="txtRem" runat="server" CssClass="form-control form-control-sm dlv-rem" MaxLength="120" /></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></div></FooterTemplate>
            </asp:Repeater>
            <div class="mt-3 d-flex gap-2">
                <asp:Button ID="btnSubmit" runat="server" Text="Submit receiving" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                <a class="btn btn-outline-primary" href="Receiving.aspx">Cancel</a>
            </div>
        </div></div>
    </asp:Panel>

    <asp:Literal ID="litRO" runat="server" />

    <h5>Deliveries expected / being followed up</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptOpen" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Delivery No.</th><th>PO No.</th><th>Supplier</th><th>Requesting dept.</th><th>Expected</th><th class="text-end">Outstanding qty</th><th>Status</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Supplier") %></td>
                    <td><%#: Eval("Client") %></td>
                    <td><%# Eval("ExpectedDate", "{0:MMM dd, yyyy}") %></td>
                    <td class="text-end"><%# Eval("Outstanding") %></td>
                    <td><%# DeliveryHtml.Badge(Eval("Status")) %></td>
                    <td><a class="btn btn-sm btn-primary" href="Receiving.aspx?id=<%# Eval("Id") %>">Receive</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="No deliveries are waiting to be received." Visible="false"></asp:Label>
    </div></div>
</div>
</asp:Content>
