<%@ Page Title="Delivery Notices" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DeliveryNotices.aspx.cs" Inherits="Drafters.Delivery.NoticesPage" %>
<%@ Import Namespace="Drafters.Delivery" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="dlv">
    <h3 class="mb-3">Delivery Notices</h3>

    <asp:Panel ID="pnlForm" runat="server" CssClass="card mb-4">
        <div class="card-body">
            <h5 class="mb-2">Record supplier delivery notice</h5>
            <div class="dlv-note">Step 1 of the process: the supplier sends a delivery notice with the <b>date, items and quantity</b>. Pick the purchase order the notice refers to.</div>
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Purchase order</label>
                    <asp:DropDownList ID="ddlPo" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPo_SelectedIndexChanged" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Expected delivery date</label>
                    <asp:TextBox ID="txtExpected" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="col-12"><asp:Literal ID="litPoInfo" runat="server" /></div>
                <div class="col-12">
                    <asp:Repeater ID="rptLines" runat="server">
                        <HeaderTemplate><div class="table-responsive"><table class="table align-middle mb-0"><thead class="table-light"><tr><th>Item</th><th>Unit</th><th class="text-end">Ordered</th><th class="text-end">Qty in notice</th></tr></thead><tbody></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><b><%#: Eval("Name") %></b><div class="dlv-sub"><%#: Eval("Sku") %></div><asp:HiddenField ID="hfSku" runat="server" Value='<%# Eval("Sku") %>' /></td>
                                <td><%#: Eval("Unit") %></td>
                                <td class="text-end"><%# Eval("Ordered") %></td>
                                <td class="text-end"><asp:TextBox ID="txtQty" runat="server" TextMode="Number" min="0" CssClass="form-control form-control-sm dlv-num ms-auto" Text='<%# Eval("Ordered") %>' /></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></div></FooterTemplate>
                    </asp:Repeater>
                </div>
                <div class="col-12">
                    <label class="form-label">Notes (optional)</label>
                    <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
            </div>
            <div class="mt-3"><asp:Button ID="btnSave" runat="server" Text="Record delivery notice" CssClass="btn btn-primary" OnClick="btnSave_Click" /></div>
        </div>
    </asp:Panel>
    <asp:Literal ID="litRO" runat="server" />

    <h5>All delivery notices</h5>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptNotices" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Notice No.</th><th>PO No.</th><th>Supplier</th><th>Requesting dept.</th><th>Notice date</th><th>Expected</th><th class="text-end">Qty</th><th>Status</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("Id") %></b></td>
                    <td><%#: Eval("PoNo") %></td>
                    <td><%#: Eval("Supplier") %></td>
                    <td><%#: Eval("Client") %></td>
                    <td><%# Eval("NoticeDate", "{0:MMM dd, yyyy}") %></td>
                    <td><%# Eval("ExpectedDate", "{0:MMM dd, yyyy}") %></td>
                    <td class="text-end"><%# Eval("NoticeTotal") %></td>
                    <td><%# DeliveryHtml.Badge(Eval("Status")) %></td>
                    <td><a class="btn btn-sm btn-outline-primary" href="DeliveryRecords.aspx?id=<%# Eval("Id") %>">View</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="No delivery notices yet." Visible="false"></asp:Label>
    </div></div>
</div>
</asp:Content>
