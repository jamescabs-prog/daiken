<%@ Page Title="Requisition" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RequestDetails.aspx.cs" Inherits="Drafters.RequestDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h3 class="mb-0"><asp:Label ID="lblMrs" runat="server" /> <asp:Label ID="lblStatus" runat="server" /></h3>
        <a class="btn btn-outline-dark btn-sm" href="Requests.aspx">&larr; Back</a>
    </div>

    <div class="card mb-3"><div class="card-body"><div class="row">
        <div class="col-md-4"><div class="text-muted small">Requester</div><asp:Label ID="lblRequester" runat="server" /></div>
        <div class="col-md-2"><div class="text-muted small">Job Order</div><asp:Label ID="lblJob" runat="server" /></div>
        <div class="col-md-3"><div class="text-muted small">Date Needed</div><asp:Label ID="lblNeeded" runat="server" /></div>
        <div class="col-md-3"><div class="text-muted small">Submitted</div><asp:Label ID="lblSubmitted" runat="server" /></div>
        <div class="col-12 mt-3"><div class="text-muted small">Purpose</div><asp:Label ID="lblPurpose" runat="server" /></div>
        <asp:Panel ID="pnlApproval" runat="server" Visible="false" CssClass="col-12 mt-3">
            <div class="text-muted small"><asp:Label ID="lblApproval" runat="server" /></div>
            <div><asp:Label ID="lblNote" runat="server" /></div>
        </asp:Panel>
    </div></div></div>

    <div class="card mb-3"><div class="card-header bg-white"><b>Requested Items</b></div>
        <asp:Repeater ID="rptLines" runat="server">
            <HeaderTemplate>
                <table class="table mb-0 align-middle">
                    <thead class="table-light"><tr><th>Tool / equipment</th><th class="text-end">Requested</th><th class="text-end">Reserved</th><th class="text-end">Issued</th><th class="text-end">Stock available now</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%#: Eval("Item.ItemCode") %> - <%#: Eval("Item.ItemName") %></td>
                    <td class="text-end"><%# Eval("QtyRequested") %> <%#: Eval("Item.Unit") %></td>
                    <td class="text-end"><%# Eval("QtyReserved") %></td>
                    <td class="text-end"><%# Eval("QtyIssued") %></td>
                    <td class="text-end"><%# Eval("Item.Available") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
    </div>

    <%-- Requester: RF returned for revision --%>
    <asp:Panel ID="pnlRevision" runat="server" Visible="false" CssClass="card mb-3 border-danger">
        <div class="card-body">
            <h5 class="text-danger">This RF was returned for revision</h5>
            <p class="mb-3"><b>Admin's reason:</b> <asp:Label ID="lblAdminReason" runat="server" /></p>
            <asp:HyperLink ID="lnkRevise" runat="server" CssClass="btn btn-primary">Revise &amp; resubmit</asp:HyperLink>
            <asp:Button ID="btnDeleteRevision" runat="server" Text="Delete request completely" CssClass="btn btn-outline-danger"
                OnClick="btnDelete_Click" OnClientClick="return confirm('Delete this requisition completely? This cannot be undone.');" />
        </div>
    </asp:Panel>

    <%-- Requester: withdraw a request that is still waiting for approval --%>
    <asp:Panel ID="pnlWithdraw" runat="server" Visible="false" CssClass="card mb-3">
        <div class="card-body d-flex justify-content-between align-items-center">
            <span>Waiting for the admin's approval.</span>
            <asp:Button ID="btnDeletePending" runat="server" Text="Withdraw &amp; delete" CssClass="btn btn-sm btn-outline-danger"
                OnClick="btnDelete_Click" OnClientClick="return confirm('Delete this requisition completely? This cannot be undone.');" />
        </div>
    </asp:Panel>

    <%-- Admin review (Grace Duran / authorized manager) --%>
    <asp:Panel ID="pnlReview" runat="server" Visible="false" CssClass="card mb-3 border-warning">
        <div class="card-body">
            <h5>Review Requisition <asp:Label ID="lblRevNo" runat="server" /></h5>
            <asp:Panel ID="pnlStock" runat="server" Visible="false" CssClass="alert alert-danger py-2">
                <b>Stock problem:</b>
                <asp:Literal ID="litStock" runat="server"></asp:Literal>
                <small>You can return this request to the requester, or approve it and the shortage will be sent to Purchasing automatically.</small>
            </asp:Panel>
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Comments (optional)</label>
                    <asp:TextBox ID="txtComments" runat="server" CssClass="form-control mb-2" MaxLength="500"></asp:TextBox>
                    <asp:Button ID="btnApprove" runat="server" Text="Approve and send to Inventory" CssClass="btn btn-success" OnClick="btnApprove_Click"
                        OnClientClick="return confirm('Approve this requisition and send it to Inventory? Items without enough stock go to Purchasing automatically.');" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Not approved? Tell the requester what to revise (required)</label>
                    <asp:TextBox ID="txtReason" runat="server" CssClass="form-control mb-2" MaxLength="500"></asp:TextBox>
                    <asp:Button ID="btnReturn" runat="server" Text="Return to requester for revision" CssClass="btn btn-danger" OnClick="btnReturn_Click" />
                </div>
            </div>
        </div>
    </asp:Panel>

    <%-- Issuance slips / release --%>
    <asp:Panel ID="pnlSlips" runat="server" Visible="false" CssClass="card mb-3">
        <div class="card-header bg-white"><b>Issuance Slips (IS) &amp; Acknowledgement Receipts (AR)</b></div>
        <asp:Repeater ID="rptSlips" runat="server" OnItemCommand="rptSlips_ItemCommand">
            <HeaderTemplate>
                <table class="table mb-0 align-middle">
                    <thead class="table-light"><tr><th>IS No.</th><th>Items</th><th>Generated</th><th>AR No. / Released</th><th></th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%#: Eval("IsNo") %></td>
                    <td><%# SlipItemsHtml(Container.DataItem) %></td>
                    <td><%# Eval("GeneratedAt", "{0:MMM dd, h:mm tt}") %></td>
                    <td><%# ReleaseInfo(Container.DataItem) %></td>
                    <td style="min-width:300px">
                        <a class="btn btn-sm btn-outline-secondary" target="_blank" href="SlipPrint.aspx?id=<%# Eval("SlipId") %>">Print</a>
                        <asp:Panel ID="pnlRelease" runat="server" CssClass="d-inline-flex gap-1 mt-1" Visible='<%# CanRelease(Container.DataItem) %>'>
                            <asp:TextBox ID="txtReceivedBy" runat="server" CssClass="form-control form-control-sm" Text='<%# RequesterName %>'></asp:TextBox>
                            <asp:Button ID="btnRelease" runat="server" Text="Release items" CssClass="btn btn-sm btn-primary text-nowrap" CommandName="Release" CommandArgument='<%# Eval("SlipId") %>' />
                        </asp:Panel>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
    </asp:Panel>

    <%-- Replenishment --%>
    <asp:Panel ID="pnlReps" runat="server" Visible="false" CssClass="card mb-3">
        <div class="card-header bg-white"><b>Purchase / Replenishment Requests (RepF)</b></div>
        <asp:Repeater ID="rptReps" runat="server">
            <HeaderTemplate>
                <table class="table mb-0">
                    <thead class="table-light"><tr><th>RepF No.</th><th>Item</th><th class="text-end">Still short</th><th class="text-end">Received</th><th>Status</th><th>Reminders</th></tr></thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%#: Eval("RepNo") %></td>
                    <td><%#: Eval("Item.ItemName") %></td>
                    <td class="text-end"><%# Eval("QtyShort") %></td>
                    <td class="text-end"><%# Eval("QtyReceived") %></td>
                    <td><span class="badge bg-<%# Drafters.Services.Ui.Badge((string)Eval("Status")) %>"><%#: Eval("Status") %></span></td>
                    <td><%# Eval("ReminderCount") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate></tbody></table></FooterTemplate>
        </asp:Repeater>
    </asp:Panel>

    <div class="card"><div class="card-header bg-white"><b>Transaction history</b></div>
        <ul class="list-group list-group-flush">
            <asp:Repeater ID="rptLogs" runat="server">
                <ItemTemplate>
                    <li class="list-group-item"><b><%#: Eval("Action") %></b> &mdash; <%#: Eval("Details") %>
                        <div class="text-muted small"><%# Eval("CreatedAt", "{0:MMM dd, yyyy h:mm tt}") %> <%#: LogUser(Container.DataItem) %></div></li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </div>
</asp:Content>
