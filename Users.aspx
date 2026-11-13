<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="Drafters.Users" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h3 class="mb-0">User Management</h3>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add user" CssClass="btn btn-primary btn-sm" OnClick="btnAdd_Click" CausesValidation="false" />
    </div>

    <asp:Panel ID="pnlForm" runat="server" CssClass="card mb-4" Visible="false">
        <div class="card-body">
            <h5 class="mb-3"><asp:Label ID="lblFormTitle" runat="server" /></h5>
            <asp:HiddenField ID="hfUserId" runat="server" />
            <div class="row g-3">
                <div class="col-md-3"><label class="form-label">Employee ID</label><asp:TextBox ID="txtEmp" runat="server" CssClass="form-control" MaxLength="20" placeholder="EMP-0005" /></div>
                <div class="col-md-5"><label class="form-label">Full name</label><asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" /></div>
                <div class="col-md-4"><label class="form-label">Username</label><asp:TextBox ID="txtUser" runat="server" CssClass="form-control" MaxLength="50" /></div>
                <div class="col-md-4"><label class="form-label">Department</label><asp:TextBox ID="txtDept" runat="server" CssClass="form-control" MaxLength="100" /></div>
                <div class="col-md-4"><label class="form-label">Role</label><asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select" /></div>
                <div class="col-md-4"><label class="form-label"><asp:Label ID="lblPwdHint" runat="server" /></label><asp:TextBox ID="txtPwd" runat="server" CssClass="form-control" TextMode="Password" MaxLength="50" autocomplete="new-password" /></div>
            </div>
            <div class="mt-3 d-flex gap-2">
                <asp:Button ID="btnSave" runat="server" Text="Save user" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" OnClick="btnCancel_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <div class="card mb-3"><div class="card-body">
        <div class="row g-3 align-items-end">
            <div class="col-md-5"><label class="form-label">Search (ID, name, username)</label><asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" /></div>
            <div class="col-md-4"><label class="form-label">Role</label><asp:DropDownList ID="ddlFilterRole" runat="server" CssClass="form-select" /></div>
            <div class="col-md-3 d-flex gap-2">
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-primary" OnClick="btnClear_Click" />
            </div>
        </div>
    </div></div>

    <div class="card"><div class="table-responsive">
        <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
            <HeaderTemplate>
                <table class="table table-hover mb-0 align-middle">
                    <thead class="table-light"><tr><th>Employee ID</th><th>Name</th><th>Username</th><th>Department</th><th>Role</th><th>Status</th><th style="min-width:300px">Actions</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><b><%#: Eval("EmployeeId") %></b></td>
                    <td><%#: Eval("FullName") %></td>
                    <td><%#: Eval("Username") %></td>
                    <td><%#: Eval("Department") %></td>
                    <td><%#: RoleLabel(Eval("Role")) %></td>
                    <td><%# StatusBadge(Eval("IsActive")) %></td>
                    <td>
                        <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-primary" CommandName="Edit" CommandArgument='<%# Eval("UserId") %>' CausesValidation="false" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset password" CssClass="btn btn-sm btn-outline-warning" CommandName="Reset" CommandArgument='<%# Eval("UserId") %>' CausesValidation="false" OnClientClick="return confirm('Reset this password to Pass@123?');" />
                        <asp:Button ID="btnToggle" runat="server" Text='<%# (bool)Eval("IsActive") ? "Deactivate" : "Activate" %>' CssClass="btn btn-sm btn-outline-danger" CommandName="Toggle" CommandArgument='<%# Eval("UserId") %>' CausesValidation="false" Visible='<%# !IsSelf(Container.DataItem) %>' />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblEmpty" runat="server" CssClass="d-block text-center text-muted py-4" Text="No users found." Visible="false"></asp:Label>
    </div></div>
</asp:Content>
