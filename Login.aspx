<%@ Page Title="Sign in - Daikin Requisition" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Drafters.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<style>
    @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700;800&display=swap');

    .login-page { position:fixed; inset:0; z-index:1000; overflow:auto; display:flex; align-items:center; justify-content:center;
        padding:24px; font-family:'Poppins',system-ui,sans-serif;
        background:linear-gradient(135deg,#eaf7fe 0%,#cdeefb 50%,#a6dcf5 100%); }
    .login-page::before, .login-page::after { content:""; position:absolute; border-radius:50%; background:rgba(255,255,255,.35); pointer-events:none; }
    .login-page::before { width:420px; height:420px; top:-140px; left:-120px; }
    .login-page::after  { width:320px; height:320px; bottom:-120px; right:-90px; }

    .login-shell { position:relative; z-index:1; width:100%; max-width:1040px; min-height:580px; display:flex; background:#fff;
        border-radius:30px; overflow:hidden; box-shadow:0 30px 70px rgba(0,110,180,.28); }

    .login-left { flex:0 0 42%; padding:40px 46px; display:flex; flex-direction:column; justify-content:center; }
    .login-right { flex:1; position:relative; display:flex; align-items:flex-end; justify-content:flex-end; padding:48px 52px; color:#fff; overflow:hidden; }
    .login-right svg.wave { position:absolute; inset:0; width:100%; height:100%; }
    .login-welcome { position:relative; text-align:right; max-width:340px; }
    .login-welcome h1 { font-size:58px; font-weight:700; margin:0 0 8px; letter-spacing:-1px; text-shadow:0 4px 18px rgba(0,70,120,.25); }
    .login-welcome .sub { font-size:13px; font-weight:500; letter-spacing:.14em; text-transform:uppercase; opacity:.95; margin-bottom:8px; }
    .login-welcome p { font-size:13px; line-height:1.7; opacity:.92; margin:0; }

    .dk-logo { height:34px; width:auto; display:block; }
    .lg-avatar { width:76px; height:76px; border-radius:50%; margin:22px auto 14px; display:flex; align-items:center; justify-content:center;
        background:linear-gradient(135deg,#27b0f0,#0086cc); box-shadow:0 10px 24px rgba(0,134,204,.35); }
    .lg-title { text-align:center; font-weight:600; font-size:22px; color:#10425f; margin:0; }
    .lg-sub { text-align:center; font-size:12.5px; color:#6b8ea3; margin:4px 0 22px; }

    .lg-field { position:relative; margin-bottom:14px; }
    .lg-field > svg.ico { position:absolute; left:17px; top:50%; transform:translateY(-50%); width:18px; height:18px; color:#52a9d6; pointer-events:none; }
    .lg-input { width:100%; border:1.5px solid #cfe7f4; background:#f6fbfe; border-radius:999px; padding:13px 46px 13px 46px;
        font-family:'Poppins',sans-serif; font-size:14px; color:#10425f; outline:none; transition:border-color .2s, box-shadow .2s, background .2s; }
    .lg-input::placeholder { color:#9bb8c9; }
    .lg-input:focus { border-color:#0097e0; background:#fff; box-shadow:0 0 0 4px rgba(0,151,224,.15); }
    .lg-eye { position:absolute; right:12px; top:50%; transform:translateY(-50%); border:0; background:transparent; padding:6px; line-height:0; color:#7ab6d8; cursor:pointer; }
    .lg-eye:hover { color:#0097e0; }

    .lg-btn { width:100%; border:0; border-radius:999px; padding:13px; margin-top:6px; color:#fff; font-family:'Poppins',sans-serif; font-weight:600;
        font-size:14px; letter-spacing:.14em; cursor:pointer; background:linear-gradient(135deg,#1db1f2,#0086cc);
        box-shadow:0 10px 22px rgba(0,134,204,.38); transition:transform .15s, box-shadow .15s, filter .15s; }
    .lg-btn:hover { transform:translateY(-2px); box-shadow:0 14px 26px rgba(0,134,204,.45); filter:brightness(1.04); }
    .lg-btn:active { transform:translateY(0); }

    .lg-error { display:block; margin:0 0 14px; padding:10px 16px; border-radius:14px; background:#fff1f1; border:1px solid #ffc9c9; color:#b42318; font-size:13px; }

    .lg-demo { margin-top:20px; padding-top:14px; border-top:1px dashed #cfe7f4; text-align:center; }
    .lg-demo .lbl { font-size:11px; color:#7f9fb2; margin-bottom:8px; }
    .lg-chip { border:1px solid #bfe3f5; background:#eef8fd; color:#0a6ea5; border-radius:999px; padding:4px 11px; font-size:11px; font-weight:500;
        margin:2px; cursor:pointer; font-family:'Poppins',sans-serif; }
    .lg-chip:hover { background:#0097e0; border-color:#0097e0; color:#fff; }

    @media (max-width: 820px) {
        .login-shell { flex-direction:column-reverse; max-width:460px; }
        .login-left { flex:none; padding:32px 28px; }
        .login-right { min-height:190px; padding:26px 28px; }
        .login-welcome h1 { font-size:38px; }
        .login-welcome p { display:none; }
    }
</style>

<div class="login-page">
  <div class="login-shell">

    <%-- LEFT: sign-in form --%>
    <div class="login-left">
        <svg class="dk-logo" viewBox="0 0 460 100" xmlns="http://www.w3.org/2000/svg" role="img" aria-label="Daikin">
            <polygon points="11,0 59,25 6,48" fill="#0f1a24"></polygon>
            <polygon points="6,48 59,25 100,46 0,100" fill="#2aa9e0"></polygon>
            <text x="122" y="80" font-family="Poppins, Arial, sans-serif" font-weight="800" font-size="84" fill="#0a98de" textLength="330" lengthAdjust="spacingAndGlyphs">DAIKIN</text>
        </svg>

        <div class="lg-avatar">
            <svg width="38" height="38" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"></circle><path d="M4 21c0-4.4 3.6-7 8-7s8 2.6 8 7"></path></svg>
        </div>
        <h2 class="lg-title">Sign in</h2>
        <p class="lg-sub">Enter your Employee ID &mdash; we&rsquo;ll open the right page for you.</p>

        <asp:Label ID="lblError" runat="server" CssClass="lg-error" Visible="false"></asp:Label>

        <div class="lg-field">
            <svg class="ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="5" width="18" height="14" rx="2"></rect><circle cx="9" cy="11" r="2"></circle><path d="M6 16c.6-1.5 1.8-2 3-2s2.4.5 3 2M14 10h4M14 14h4"></path></svg>
            <asp:TextBox ID="txtEmployeeId" runat="server" ClientIDMode="Static" CssClass="lg-input" placeholder="Employee ID  (e.g. EMP-1001)" autofocus="autofocus" autocomplete="username"></asp:TextBox>
        </div>
        <div class="lg-field">
            <svg class="ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="5" y="11" width="14" height="9" rx="2"></rect><path d="M8 11V8a4 4 0 0 1 8 0v3"></path></svg>
            <asp:TextBox ID="txtPassword" runat="server" ClientIDMode="Static" TextMode="Password" CssClass="lg-input" placeholder="Password" autocomplete="current-password"></asp:TextBox>
            <button type="button" class="lg-eye" id="togglePw" aria-label="Show or hide password">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12z"></path><circle cx="12" cy="12" r="3"></circle></svg>
            </button>
        </div>

        <asp:Button ID="btnLogin" runat="server" Text="LOGIN" CssClass="lg-btn" OnClick="btnLogin_Click" />

        <div class="lg-demo">
            <div class="lbl">Demo accounts &middot; password <b>Pass@123</b> &middot; click one to fill</div>
            <button type="button" class="lg-chip" data-id="EMP-1001">EMP-1001 Employee</button>
            <button type="button" class="lg-chip" data-id="EMP-1002">EMP-1002 Employee</button>
            <button type="button" class="lg-chip" data-id="EMP-0001">EMP-0001 Admin</button>
            <button type="button" class="lg-chip" data-id="EMP-0002">EMP-0002 Inventory</button>
            <button type="button" class="lg-chip" data-id="EMP-0003">EMP-0003 Purchasing</button>
            <button type="button" class="lg-chip" data-id="EMP-0004">EMP-0004 Delivery</button>
        </div>
    </div>

    <%-- RIGHT: welcome panel --%>
    <div class="login-right">
        <svg class="wave" viewBox="0 0 620 600" preserveAspectRatio="xMidYMid slice" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
            <defs>
                <linearGradient id="wbg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#0b86d0"></stop><stop offset="1" stop-color="#2fb3ee"></stop></linearGradient>
                <linearGradient id="wr1" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#ffffff"></stop><stop offset="0.55" stop-color="#d4f0fc"></stop><stop offset="1" stop-color="#7fd0f6"></stop></linearGradient>
                <linearGradient id="wr2" x1="1" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#ffffff" stop-opacity=".55"></stop><stop offset="1" stop-color="#9ddcf8" stop-opacity=".15"></stop></linearGradient>
            </defs>
            <rect width="620" height="600" fill="url(#wbg)"></rect>
            <path d="M470 -20 C 560 120, 380 200, 340 300 C 300 400, 440 470, 380 620 L -20 620 L -20 -20 Z" fill="url(#wr2)"></path>
            <path d="M300 -20 C 420 90, 190 190, 230 310 C 270 420, 120 480, 170 620 L -20 620 L -20 -20 Z" fill="url(#wr1)"></path>
            <path d="M120 -20 C 230 100, 60 200, 110 320 C 150 420, 40 500, 70 620 L -20 620 L -20 -20 Z" fill="#ffffff" opacity=".55"></path>
            <circle cx="540" cy="90" r="60" fill="#ffffff" opacity=".10"></circle>
            <circle cx="500" cy="130" r="26" fill="#ffffff" opacity=".12"></circle>
        </svg>
        <div class="login-welcome">
            <div class="sub">Requisition System</div>
            <h1>Welcome.</h1>
            <p>Request tools and equipment, follow every approval and keep your inventory in order &mdash; all in one place.</p>
        </div>
    </div>

  </div>
</div>

<script>
    (function () {
        var id = document.getElementById('txtEmployeeId');
        var pw = document.getElementById('txtPassword');
        document.getElementById('togglePw').addEventListener('click', function () {
            pw.type = (pw.type === 'password') ? 'text' : 'password';
        });
        Array.prototype.forEach.call(document.querySelectorAll('.lg-chip'), function (b) {
            b.addEventListener('click', function () { id.value = b.getAttribute('data-id'); pw.value = 'Pass@123'; pw.focus(); });
        });
    })();
</script>
</asp:Content>
