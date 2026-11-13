<%@ Page Title="Inventory" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Inventory.aspx.cs" Inherits="Drafters.Inventory" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h3 class="mb-0">Inventory</h3>
        <div class="d-flex gap-2">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control form-control-sm" placeholder="Search item..."></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-sm btn-outline-dark" OnClick="btnSearch_Click" />
            <a class="btn btn-sm btn-dark text-nowrap" href="ItemEdit.aspx">+ New item</a>
        </div>
    </div>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptItems" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Code</th><th>Item</th><th>Unit</th><th class="text-end">On hand</th><th class="text-end">Reserved</th><th class="text-end">Available</th><th class="text-end">Reorder lvl</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr class="<%# LowClass(Container.DataItem) %>">
                    <td><%#: Eval("ItemCode") %></td>
                    <td><%#: Eval("ItemName") %> <%# InactiveBadge(Container.DataItem) %></td>
                    <td><%#: Eval("Unit") %></td>
                    <td class="text-end"><%# Eval("Quantity") %></td>
                    <td class="text-end"><%# Eval("ReservedQty") %></td>
                    <td class="text-end"><b><%# Eval("Available") %></b></td>
                    <td class="text-end"><%# Eval("ReorderLevel") %></td>
                    <td><a class="btn btn-sm btn-outline-primary" href="ItemEdit.aspx?id=<%# Eval("ItemId") %>">Edit</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
    </div></div>
    <small class="text-muted">Highlighted rows are at or below their reorder level.</small>
</asp:Content>
