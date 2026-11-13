<%@ Page Title="Replenishment" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Replenishment.aspx.cs" Inherits="Drafters.Replenishment" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h3 class="mb-0">Purchase / Replenishment Requests (RepF)</h3>
        <asp:CheckBox ID="chkAll" runat="server" Text="Show all (including received)" AutoPostBack="true" OnCheckedChanged="chkAll_CheckedChanged" />
    </div>
    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptReps" runat="server" OnItemCommand="rptReps_ItemCommand">
            <HeaderTemplate>
                <table class="table mb-0 align-middle">
                    <thead class="table-light"><tr><th>RepF No.</th><th>For MRS</th><th>Item</th><th class="text-end">Still short</th><th class="text-end">Received</th><th>Status</th><th>Reminders</th><th style="min-width:260px">Action</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%#: Eval("RepNo") %></td>
                    <td><%# MrsCell(Container.DataItem) %></td>
                    <td><%#: Eval("Item.ItemName") %></td>
                    <td class="text-end"><%# Eval("QtyShort") %></td>
                    <td class="text-end"><%# Eval("QtyReceived") %></td>
                    <td><span class="badge bg-<%# Drafters.Services.Ui.Badge((string)Eval("Status")) %>"><%#: Eval("Status") %></span></td>
                    <td><%# Eval("ReminderCount") %> <%# LastReminder(Container.DataItem) %></td>
                    <td>
                        <asp:Panel ID="pnlReceive" runat="server" CssClass="d-inline-flex gap-1" Visible='<%# CanReceive(Container.DataItem) %>'>
                            <asp:TextBox ID="txtQty" runat="server" TextMode="Number" CssClass="form-control form-control-sm" Width="80px" Text='<%# Eval("QtyShort") %>'></asp:TextBox>
                            <asp:Button ID="btnReceive" runat="server" Text="Receive stock" CssClass="btn btn-sm btn-success text-nowrap" CommandName="Receive" CommandArgument='<%# Eval("RepId") %>' />
                        </asp:Panel>
                        <asp:Button ID="btnRemind" runat="server" Text="Send reminder" CssClass="btn btn-sm btn-outline-warning" CommandName="Remind" CommandArgument='<%# Eval("RepId") %>' Visible='<%# CanRemind(Container.DataItem) %>' />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="Nothing here." Visible="false"></asp:Label>
    </div></div>
</asp:Content>
