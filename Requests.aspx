<%@ Page Title="Requisitions" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Requests.aspx.cs" Inherits="Drafters.Requests" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h3 class="mb-0"><asp:Label ID="lblTitle" runat="server" /></h3>
        <div class="d-flex gap-2 align-items-center">
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                <asp:ListItem Value="" Text="All statuses"></asp:ListItem>
                <asp:ListItem Value="Pending Approval"></asp:ListItem>
                <asp:ListItem Value="For Revision"></asp:ListItem>
                <asp:ListItem Value="Ready for Release"></asp:ListItem>
                <asp:ListItem Value="Pending - Awaiting Stock"></asp:ListItem>
                <asp:ListItem Value="Completed"></asp:ListItem>
                <asp:ListItem Value="Rejected"></asp:ListItem>
            </asp:DropDownList>
            <div class="form-check text-nowrap">
                <asp:CheckBox ID="chkArchived" runat="server" Text="Show archive" AutoPostBack="true" OnCheckedChanged="Filter_Changed" />
            </div>
            <asp:HyperLink ID="lnkNew" runat="server" NavigateUrl="~/RequestForm.aspx" CssClass="btn btn-dark btn-sm text-nowrap" Visible="false">+ New Requisition</asp:HyperLink>
        </div>
    </div>

    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptRequests" runat="server">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>MRS No.</th><th>Requester</th><th>Purpose</th><th>Date Needed</th><th>Status</th><th>Created</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("MrsNo") %></b></td>
                    <td><%#: Eval("Requester.FullName") %><br /><small class="text-muted"><%#: Eval("Department") %></small></td>
                    <td><%#: Eval("Purpose") %></td>
                    <td><%# Eval("DateNeeded", "{0:MMM dd, yyyy}") %></td>
                    <td><span class="badge bg-<%# Drafters.Services.Ui.Badge((string)Eval("Status")) %>"><%#: Eval("Status") %></span></td>
                    <td><%# Eval("CreatedAt", "{0:MMM dd, yyyy h:mm tt}") %></td>
                    <td><a class="btn btn-sm btn-outline-primary" href="RequestDetails.aspx?id=<%# Eval("RequisitionId") %>">Open</a></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="No requisitions found." Visible="false"></asp:Label>
    </div></div>
</asp:Content>
