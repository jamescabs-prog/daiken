<%@ Page Title="Notifications" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="Drafters.Notifications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-3">Notifications</h3>
    <div class="list-group">
        <asp:Repeater ID="rptNotifs" runat="server">
            <ItemTemplate>
                <a class="list-group-item list-group-item-action <%# BoldClass(Container.DataItem) %>" href="<%#: Eval("Link") %>">
                    <%#: Eval("Message") %>
                    <div class="small text-muted fw-normal"><%# Eval("CreatedAt", "{0:MMM dd, yyyy h:mm tt}") %></div>
                </a>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="list-group-item text-muted" Text="No notifications." Visible="false"></asp:Label>
    </div>
</asp:Content>
