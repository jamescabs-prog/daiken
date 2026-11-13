<%@ Page Title="Requisition Form" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RequestForm.aspx.cs" Inherits="Drafters.RequestForm" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-3"><asp:Label ID="lblHeading" runat="server" /></h3>

    <asp:Panel ID="pnlRevise" runat="server" Visible="false" CssClass="alert alert-danger">
        <b>Returned by the admin:</b> <asp:Label ID="lblReason" runat="server" /><br />
        <small>Make the corrections below, then resubmit.</small>
    </asp:Panel>

    <div class="card mb-3">
        <div class="card-header bg-white"><b>Requester information</b></div>
        <div class="card-body"><div class="row g-3">
            <div class="col-md-3"><div class="text-muted small">Employee ID</div><b><asp:Label ID="lblEmpId" runat="server" /></b></div>
            <div class="col-md-4"><div class="text-muted small">Full name</div><b><asp:Label ID="lblFullName" runat="server" /></b></div>
            <div class="col-md-3"><div class="text-muted small">Department</div><b><asp:Label ID="lblDepartment" runat="server" /></b></div>
            <div class="col-md-2"><div class="text-muted small">Date requested</div><b><asp:Label ID="lblToday" runat="server" /></b></div>
        </div></div>
    </div>

    <asp:Label ID="lblFormError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>

    <div class="card mb-3"><div class="card-body"><div class="row g-3">
        <div class="col-md-4">
            <label class="form-label">Job Order No. (optional)</label>
            <asp:TextBox ID="txtJobOrder" runat="server" CssClass="form-control" MaxLength="30"></asp:TextBox>
        </div>
        <div class="col-md-4">
            <label class="form-label">Date Needed</label>
            <asp:TextBox ID="txtDateNeeded" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
        </div>
        <div class="col-md-12">
            <label class="form-label">Purpose</label>
            <asp:TextBox ID="txtPurpose" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" MaxLength="500"></asp:TextBox>
        </div>
    </div></div></div>

    <div class="card mb-3"><div class="card-body">
        <div class="d-flex justify-content-between mb-2">
            <h5 class="mb-0">Tools / equipment needed <small class="text-muted fs-6">(stock shown is live)</small></h5>
            <button type="button" id="addLine" class="btn btn-sm btn-outline-dark">+ Add item</button>
        </div>
        <table class="table align-middle" id="lines">
            <thead><tr><th style="width:50%">Tool / equipment</th><th style="width:15%">Quantity</th><th>Available now</th><th></th></tr></thead>
            <tbody></tbody>
        </table>
    </div></div>

    <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-dark" OnClick="btnSubmit_Click" />
    <a class="btn btn-link" href="Requests.aspx">Cancel</a>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsContent" runat="server">
<script>
    var items = <%= ItemsJson %>;
    var initial = <%= InitialJson %>;
    var tbody = document.querySelector('#lines tbody');

    function findItem(id) { for (var i = 0; i < items.length; i++) if (items[i].ItemId == id) return items[i]; return null; }

    function buildOptions(selected) {
        var h = '<option value="0">-- select item --</option>';
        items.forEach(function (it) {
            h += '<option value="' + it.ItemId + '"' + (it.ItemId == selected ? ' selected' : '') + '>' + it.ItemCode + ' - ' + it.ItemName + '</option>';
        });
        return h;
    }

    function showAvail(tr) {
        var it = findItem(tr.querySelector('select').value);
        var cell = tr.querySelector('.avail');
        var qty = parseInt(tr.querySelector('input').value || '0', 10);
        if (!it) { cell.innerHTML = '-'; return; }
        var isShort = qty > it.Available;
        cell.innerHTML = '<span class="' + (isShort ? 'text-danger' : 'text-success') + '">' + it.Available + ' ' + it.Unit + '</span>' +
            (isShort ? ' <small class="text-danger">(short - a purchase request will be auto-created)</small>' : '');
    }

    function addRow(itemId, qty) {
        var tr = document.createElement('tr');
        tr.innerHTML = '<td><select name="lineItem" class="form-select">' + buildOptions(itemId || 0) + '</select></td>' +
            '<td><input name="lineQty" type="number" min="1" value="' + (qty || 1) + '" class="form-control" /></td>' +
            '<td class="avail">-</td>' +
            '<td><button type="button" class="btn btn-sm btn-outline-danger">&times;</button></td>';
        tr.querySelector('select').addEventListener('change', function () { showAvail(tr); });
        tr.querySelector('input').addEventListener('input', function () { showAvail(tr); });
        tr.querySelector('button').addEventListener('click', function () { tr.remove(); });
        tbody.appendChild(tr);
        showAvail(tr);
    }

    document.getElementById('addLine').addEventListener('click', function () { addRow(0, 1); });
    if (initial && initial.length) initial.forEach(function (l) { addRow(l.ItemId, l.Quantity); }); else addRow(0, 1);

    // real-time stock refresh every 15 seconds
    setInterval(function () {
        fetch('Stock.ashx', { credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                items = data;
                Array.prototype.forEach.call(tbody.querySelectorAll('tr'), showAvail);
            }).catch(function () { });
    }, 15000);
</script>
</asp:Content>
