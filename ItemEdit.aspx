<%@ Page Title="Item" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ItemEdit.aspx.cs" Inherits="Drafters.ItemEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-3"><asp:Label ID="lblHeading" runat="server" /></h3>
    <asp:Label ID="lblMsg" runat="server" CssClass="alert alert-danger d-block" Visible="false" style="max-width:640px"></asp:Label>
    <div class="card" style="max-width:640px"><div class="card-body">
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label">Item Code</label><asp:TextBox ID="txtCode" runat="server" CssClass="form-control" MaxLength="30"></asp:TextBox></div>
            <div class="col-md-8"><label class="form-label">Item Name</label><asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="150"></asp:TextBox></div>
            <div class="col-md-4"><label class="form-label">Unit</label><asp:TextBox ID="txtUnit" runat="server" CssClass="form-control" MaxLength="20"></asp:TextBox></div>
            <div class="col-md-4"><label class="form-label">Qty On Hand</label><asp:TextBox ID="txtQty" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-4"><label class="form-label">Reorder Level</label><asp:TextBox ID="txtReorder" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-12"><div class="form-check"><asp:CheckBox ID="chkActive" runat="server" Text="Active" /></div></div>
        </div>
        <div class="mt-3">
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-dark" OnClick="btnSave_Click" />
            <a class="btn btn-link" href="Inventory.aspx">Cancel</a>
        </div>
    </div></div>
</asp:Content>
