(function () {
  'use strict';

  var KEY = 'talukder-sales-demo-v2', SESSION = 'talukder-sales-demo-uid';
  var D = window.DEMO;
  var A = window.APP = { routes: [], actions: {}, forms: {}, modals: {}, onChange: [], onInput: [] };
  var state = load();
  A.state = state;
  var ui = A.ui = { period: '7', invStatus: 'all', invQ: '', invPage: 1, invDays: '0', invFrom: '', invTo: '', colPage: 1, cart: { custId: null, lines: {}, override: false }, prodCat: 'all', prodQ: '', custQ: '', custPage: 1, custFind: '', picked: {}, modal: null, modalImg: null, planDate: null, auditPage: 1, auditQ: '', auditType: 'all', stFrom: '', stTo: '', noteFilter: 'all', ordQ: '' };

  // ---------- storage ----------
  function load() {
    try { var s = JSON.parse(localStorage.getItem(KEY)); if (s && s.v === 2) return s; } catch (e) { /* fall through */ }
    var s2 = D.seed(); save(s2); return s2;
  }
  function save(s) { try { localStorage.setItem(KEY, JSON.stringify(s || state)); } catch (e) { /* ignore */ } }
  function resetState() { var n = D.seed(); Object.keys(state).forEach(function (k) { delete state[k]; }); Object.keys(n).forEach(function (k) { state[k] = n[k]; }); save(); }

  // ---------- helpers ----------
  var $ = function (s, r) { return (r || document).querySelector(s); };
  var esc = function (s) { return String(s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); };
  var money = function (n) { return '৳ ' + Math.round(n).toLocaleString('en-IN'); };
  var short = function (n) { return n >= 100000 ? (n / 100000).toFixed(n % 100000 ? 1 : 0) + 'L' : n >= 1000 ? Math.round(n / 1000) + 'k' : String(Math.round(n)); };
  var MON = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  var dt = function (iso) { var d = new Date(iso + 'T00:00:00'); return d.getDate() + ' ' + MON[d.getMonth()]; };
  var today = function () { return D.day(0); };
  var cust = function (id) { return state.customers.filter(function (c) { return c.id === id; })[0]; };
  var prod = function (id) { return state.products.filter(function (p) { return p.id === id; })[0]; };
  var cat = function (id) { return state.categories.filter(function (c) { return c.id === id; })[0] || { id: id, name: 'Other', cls: 'cat-0', color: '#8F8294' }; };
  var initials = function (n) { return n.split(/\s+/).slice(0, 2).map(function (w) { return w[0]; }).join('').toUpperCase(); };
  var linesTotal = function (ls) { return ls.reduce(function (s, l) { return s + l.qty * l.price; }, 0); };
  var creditTotal = function (i) { return state.returns.filter(function (r) { return r.invId === i.id; }).reduce(function (t, r) { return t + r.amount; }, 0); };
  var invTotal = function (i) { return linesTotal(i.lines) - (i.discount || 0) - creditTotal(i); };
  var invPaid = function (i) { return state.collections.filter(function (c) { return c.invId === i.id; }).reduce(function (s, c) { return s + c.amount; }, 0); };
  var invDue = function (i) { return Math.max(0, invTotal(i) - invPaid(i)); };
  var invStatus = function (i) { var p = invPaid(i), t = invTotal(i); return p >= t ? 'paid' : p > 0 ? 'partial' : 'unpaid'; };
  var ageDays = function (iso) { return Math.round((new Date(today() + 'T00:00:00') - new Date(iso + 'T00:00:00')) / 864e5); };
  var sum = function (a, f) { return a.reduce(function (s, x) { return s + f(x); }, 0); };
  var roleOf = function (name) { return state.roles.filter(function (r) { return r.name === name; })[0]; };
  var me = function () { var id = +sessionStorage.getItem(SESSION); return state.users.filter(function (u) { return u.id === id; })[0]; };
  var can = function (k) { var u = me(); if (!u) return false; var r = roleOf(u.role); return !!r && (r.locked || r.perms.indexOf(k) >= 0); };
  var pad = function (n) { return String(n).padStart(2, '0'); };
  var nowIso = function () { var d = new Date(); return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate()) + 'T' + pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds()); };
  var fmtTime = function (hhmm) { var p = hhmm.split(':'), h = +p[0]; return (h % 12 || 12) + ':' + p[1] + ' ' + (h < 12 ? 'AM' : 'PM'); };
  var dtt = function (iso) { var d = new Date(iso); return d.getDate() + ' ' + MON[d.getMonth()] + ', ' + fmtTime(pad(d.getHours()) + ':' + pad(d.getMinutes())); };
  var log = function (action, detail) { state.audit.push({ id: ++state.seq.audit, time: nowIso(), user: me() ? me().name : 'System', action: action, detail: detail }); };
  var notify = function (title, body) { state.notifications.push({ id: ++state.seq.note, time: nowIso(), title: title, body: body, read: false }); };
  function windowStatus() {
    var w = state.window, d = new Date(), hm = pad(d.getHours()) + ':' + pad(d.getMinutes());
    var open = !w.enforce || (hm >= w.open && hm < w.close);
    return { open: open, enforce: w.enforce, text: open ? (w.enforce ? 'Order window open until ' + fmtTime(w.close) : 'Order window is not restricted') : 'Order window closed. Opens at ' + fmtTime(w.open), close: w.enforce ? fmtTime(w.close) : 'No limit' };
  }
  function csv(name, rows) {
    var body = rows.map(function (r) { return r.map(function (c) { var t = String(c == null ? '' : c); return /[",\n]/.test(t) ? '"' + t.replace(/"/g, '""') + '"' : t; }).join(','); }).join('\r\n');
    var a = document.createElement('a'); a.href = URL.createObjectURL(new Blob(['\ufeff' + body], { type: 'text/csv' })); a.download = name; document.body.appendChild(a); a.click(); a.remove();
    toast('Downloaded ' + name + ' (opens in Excel)');
  }
  var isoOf = function (offset) { return D.day(offset); };
  var inRange = function (iso, from, to) { return iso >= from && iso <= to; };

  function thumb(p) {
    if (p.imgData) return '<img class="thumb" src="' + p.imgData + '" alt="">';
    if (p.img) return '<img class="thumb" src="img/' + p.img + '.jpg" alt="">';
    return '<div class="tile ' + cat(p.cat).cls + '" style="width:56px;height:56px">' + initials(p.name) + '</div>';
  }

  var ICONS = {
    dashboard: '<rect x="3" y="3" width="7" height="9" rx="1.5"/><rect x="14" y="3" width="7" height="5" rx="1.5"/><rect x="14" y="12" width="7" height="9" rx="1.5"/><rect x="3" y="16" width="7" height="5" rx="1.5"/>',
    reports: '<path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/>',
    'new-order': '<circle cx="12" cy="12" r="9"/><path d="M12 8v8M8 12h8"/>',
    orders: '<path d="M6 3h12l2 4v13a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V7z"/><path d="M4 7h16M9 11h6"/>',
    invoices: '<path d="M7 3h10a1 1 0 0 1 1 1v17l-3-2-3 2-3-2-3 2V4a1 1 0 0 1 1-1z"/><path d="M9 8h6M9 12h6"/>',
    collections: '<rect x="3" y="6" width="18" height="12" rx="2"/><circle cx="12" cy="12" r="2.5"/>',
    products: '<path d="M21 8l-9-5-9 5 9 5z"/><path d="M3 8v8l9 5 9-5V8M12 13v8"/>',
    customers: '<circle cx="9" cy="8" r="3.5"/><path d="M2.5 20a6.5 6.5 0 0 1 13 0"/><path d="M17 4.5a3.5 3.5 0 0 1 0 7M18.5 14a6.5 6.5 0 0 1 3 6"/>',
    logout: '<path d="M15 4h4a1 1 0 0 1 1 1v14a1 1 0 0 1-1 1h-4M10 16l4-4-4-4M14 12H3"/>',
    clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    print: '<path d="M6 9V3h12v6M6 18H4a1 1 0 0 1-1-1v-6a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v6a1 1 0 0 1-1 1h-2"/><rect x="6" y="14" width="12" height="7"/>',
    production: '<rect x="3" y="4" width="18" height="16" rx="2"/><path d="M3 9h18M8 14h8"/>',
    roles: '<path d="M12 3l8 3v6c0 4.5-3.2 8-8 9-4.8-1-8-4.5-8-9V6z"/><path d="M9 12l2 2 4-4"/>',
    bell: '<path d="M6 9a6 6 0 0 1 12 0c0 6 2.5 7.5 2.5 7.5h-17S6 15 6 9zM10 20a2 2 0 0 0 4 0"/>',
    notices: '<path d="M3 11v3a1 1 0 0 0 1 1h2l5 4V7L6 11H4a1 1 0 0 0-1 0zM15 9a4 4 0 0 1 0 6M18 6a8 8 0 0 1 0 12"/>',
    list: '<path d="M8 6h13M8 12h13M8 18h13M3.5 6h.01M3.5 12h.01M3.5 18h.01"/>',
    lock: '<rect x="5" y="11" width="14" height="9" rx="2"/><path d="M8 11V8a4 4 0 0 1 8 0v3"/>',
    tag: '<path d="M3 12V4a1 1 0 0 1 1-1h8l9 9-9 9z"/><circle cx="8" cy="8" r="1.4"/>',
    download: '<path d="M12 4v11M7 11l5 5 5-5M4 20h16"/>',
    undo: '<path d="M9 14L4 9l5-5M4 9h10a6 6 0 0 1 0 12h-3"/>',
    trash: '<path d="M4 7h16M9 7V4h6v3M6 7l1 13h10l1-13"/>'
  };
  var icon = function (n, s, w) { return '<svg width="' + (s || 20) + '" height="' + (s || 20) + '" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="' + (w || 1.8) + '" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + (ICONS[n] || '') + '</svg>'; };

  function toast(msg, err) {
    var t = document.createElement('div'); t.className = 'toast' + (err ? ' err' : ''); t.textContent = msg;
    $('#toasts').appendChild(t); setTimeout(function () { t.remove(); }, 3200);
  }

  // ---------- data selectors ----------
  function stats(from, to) {
    var inv = state.invoices.filter(function (i) { return inRange(i.date, from, to); });
    var col = state.collections.filter(function (c) { return inRange(c.date, from, to); });
    return { inv: inv, billed: sum(inv, invTotal), collected: sum(col, function (c) { return c.amount; }) };
  }
  function outstanding() { return sum(state.invoices, invDue); }
  function waiting() { return state.orders.filter(function (o) { return o.status === 'waiting'; }); }
  function custBilled(id) { return sum(state.invoices.filter(function (i) { return i.custId === id; }), invTotal); }
  function custDue(id) { return sum(state.invoices.filter(function (i) { return i.custId === id; }), invDue); }

  // ---------- charts ----------
  function lineChart(days) {
    var W = 700, H = 230, L = 46, B = 26, T = 10, R = 22;
    var rows = days.map(function (iso) { var s = stats(iso, iso); return { iso: iso, billed: s.billed, collected: s.collected }; });
    var max = Math.max.apply(null, rows.map(function (r) { return Math.max(r.billed, r.collected); }).concat([1000]));
    var step = Math.pow(10, Math.floor(Math.log10(max / 3))); var nice = [1, 2, 2.5, 5, 10].map(function (m) { return m * step; }).filter(function (v) { return v * 4 >= max; })[0] || step * 10;
    var top = nice * 4, pw = W - L - R, ph = H - T - B;
    var x = function (i) { return L + (rows.length === 1 ? pw / 2 : i * pw / (rows.length - 1)); };
    var y = function (v) { return T + ph - v / top * ph; };
    var path = function (k) { return rows.map(function (r, i) { return (i ? 'L' : 'M') + x(i).toFixed(1) + ' ' + y(r[k]).toFixed(1); }).join(' '); };
    var g = '';
    for (var i = 0; i <= 4; i++) { var v = nice * i; g += '<line x1="' + L + '" x2="' + (W - R) + '" y1="' + y(v) + '" y2="' + y(v) + '" stroke="#F3EBD6"/><text x="' + (L - 8) + '" y="' + (y(v) + 4) + '" text-anchor="end" font-size="11" fill="#8F8294">' + short(v) + '</text>'; }
    var every = Math.ceil(rows.length / 7), xl = '';
    rows.forEach(function (r, i) { if (i % every === 0 || i === rows.length - 1) xl += '<text x="' + x(i) + '" y="' + (H - 6) + '" text-anchor="middle" font-size="11" fill="#8F8294">' + dt(r.iso) + '</text>'; });
    var data = encodeURIComponent(JSON.stringify(rows.map(function (r) { return [dt(r.iso), Math.round(r.billed), Math.round(r.collected)]; })));
    return '<div class="chart-wrap" data-chart="' + data + '" data-l="' + L + '" data-w="' + W + '" data-r="' + R + '">' +
      '<svg viewBox="0 0 ' + W + ' ' + H + '" role="img" aria-label="Line chart of daily billed and collected amounts">' + g + xl +
      '<path d="' + path('billed') + ' L' + x(rows.length - 1) + ' ' + y(0) + ' L' + x(0) + ' ' + y(0) + ' Z" fill="#E5412D" fill-opacity=".1"/>' +
      '<path d="' + path('billed') + '" fill="none" stroke="#E5412D" stroke-width="2.2" stroke-linejoin="round"/>' +
      '<path d="' + path('collected') + '" fill="none" stroke="#1F9E96" stroke-width="2.2" stroke-linejoin="round"/>' +
      '<line class="xh" y1="' + T + '" y2="' + (H - B) + '" stroke="#8F8294" stroke-dasharray="3 3" opacity="0"/></svg>' +
      '<div class="tip"></div></div>';
  }

  function donut(parts) {
    var tot = sum(parts, function (p) { return p.v; }) || 1, R = 60, C = 2 * Math.PI * R, off = 0, arcs = '';
    parts.forEach(function (p) { var len = p.v / tot * C; arcs += '<circle cx="80" cy="80" r="' + R + '" fill="none" stroke="' + p.color + '" stroke-width="22" stroke-dasharray="' + len.toFixed(1) + ' ' + (C - len).toFixed(1) + '" stroke-dashoffset="' + (-off).toFixed(1) + '" transform="rotate(-90 80 80)"/>'; off += len; });
    return '<div class="donut"><svg viewBox="0 0 160 160" width="160" height="160" role="img" aria-label="Sales by category">' + arcs + '</svg><div class="mid"><b class="num" style="font-size:20px">' + short(tot) + '</b><span class="lbl">billed</span></div></div>';
  }

  // ---------- layout ----------
  var NAV = [
    ['Overview', [['Notices', '#/notices', 'notices', null], ['Dashboard', '#/dashboard', 'dashboard', 'dashboard'], ['KPI report', '#/reports', 'reports', 'reports'], ['Notifications', '#/notifications', 'bell', null]]],
    ['Sell', [['New order', '#/orders/new', 'new-order', 'order.create'], ['Orders to invoice', '#/orders', 'orders', 'order.view'], ['Invoices', '#/invoices', 'invoices', 'invoice.view'], ['Collections', '#/collections', 'collections', 'collection.view']]],
    ['Operate', [['Production plan', '#/production', 'production', 'production'], ['Products', '#/products', 'products', 'products'], ['Categories', '#/categories', 'tag', 'products'], ['Customers', '#/customers', 'customers', 'customers']]],
    ['Admin', [['Users', '#/users', 'customers', 'users'], ['Roles', '#/roles', 'roles', 'roles'], ['Order window', '#/window', 'clock', 'window'], ['Audit log', '#/audit', 'list', 'audit']]]
  ];

  function layout(route, body) {
    var wc = waiting().length, unread = state.notifications.filter(function (n) { return !n.read; }).length, u = me();
    var nav = NAV.map(function (g) {
      var items = g[1].filter(function (i) { return !i[3] || can(i[3]); });
      if (!items.length) return '';
      return '<div class="nav-group"><div class="nav-label">' + g[0] + '</div>' + items.map(function (i) {
        var on = route.nav === i[1];
        var n = i[1] === '#/orders' ? wc : i[1] === '#/notifications' ? unread : 0;
        var badge = n ? '<span class="badge">' + n + '</span>' : '';
        return '<a href="' + i[1] + '" class="' + (on ? 'on' : '') + '"' + (on ? ' aria-current="page"' : '') + '>' + icon(i[2]) + '<span class="grow">' + i[0] + '</span>' + badge + '</a>';
      }).join('') + '</div>';
    }).join('');
    return '<div class="shell"><aside class="side"><a class="logo-tile" href="#/notices" aria-label="Talukder Foods home"><img src="img/logo-purple.jpg" alt="Talukder Foods"></a>' +
      '<nav class="nav" aria-label="Main">' + nav + '</nav>' +
      '<div class="me"><div class="avatar">' + initials(u.name) + '</div><div class="who"><b>' + esc(u.name) + '</b><small>' + esc(u.role) + '</small></div><button data-act="logout" aria-label="Sign out" title="Sign out">' + icon('logout', 18) + '</button></div></aside>' +
      '<main class="content"><div class="demo-bar no-print"><span><b>Live demo.</b> Sample data only. Nothing leaves your browser.</span><button data-act="reset">Reset demo data</button></div>' + body + '</main></div>';
  }

  function head(title, sub, actions) {
    return '<header class="page-head"><div><h1 class="page-title">' + title + '</h1>' + (sub ? '<div class="page-sub">' + sub + '</div>' : '') + '</div>' + (actions ? '<div class="actions">' + actions + '</div>' : '') + '</header>';
  }

  function pager(page, total, per, act) {
    var pages = Math.max(1, Math.ceil(total / per)); if (pages === 1) return '';
    var b = '<a class="' + (page <= 1 ? 'off' : '') + '" data-act="' + act + '" data-p="' + (page - 1) + '" aria-label="Previous page">&lsaquo;</a>';
    for (var i = 1; i <= pages; i++) b += '<a class="' + (i === page ? 'on' : '') + '" data-act="' + act + '" data-p="' + i + '">' + i + '</a>';
    b += '<a class="' + (page >= pages ? 'off' : '') + '" data-act="' + act + '" data-p="' + (page + 1) + '" aria-label="Next page">&rsaquo;</a>';
    return '<div class="pager-bar"><span class="muted mini">Showing ' + ((page - 1) * per + 1) + '–' + Math.min(page * per, total) + ' of ' + total + '</span><div class="pager">' + b + '</div></div>';
  }

  function delta(cur, prev) {
    if (!prev) return '';
    var c = (cur - prev) / prev * 100, up = c >= 0;
    return '<span class="delta ' + (up ? 'up-good' : 'up-bad') + '">' + (up ? '▲ ' : '▼ ') + Math.abs(Math.round(c)) + '%</span>';
  }

  // ---------- pages ----------
  function pDashboard() {
    var p = ui.period, n = p === '1' ? 1 : +p;
    var from = isoOf(n - 1), to = today(), pf = isoOf(2 * n - 1), pt = isoOf(n);
    var s = stats(from, to), ps = stats(pf, pt), out = outstanding(), w = waiting();
    var rate = s.billed ? Math.min(100, Math.round(s.collected / s.billed * 100)) : 0;
    var span = Math.max(n, 14), days = []; for (var i = span - 1; i >= 0; i--) days.push(isoOf(i));
    var over30 = state.invoices.filter(function (i) { return invDue(i) > 0 && ageDays(i.date) > 30; });
    var owing = {}; state.invoices.forEach(function (i) { if (invDue(i) > 0) owing[i.custId] = 1; });
    var top = state.customers.map(function (c) { return { name: c.name, v: sum(s.inv.filter(function (i) { return i.custId === c.id; }), invTotal) }; }).filter(function (x) { return x.v > 0; }).sort(function (a, b) { return b.v - a.v; }).slice(0, 5);
    var maxTop = top.length ? top[0].v : 1;
    var byCat = state.categories.map(function (c) { return { name: c.name, color: c.color, v: sum(s.inv, function (i) { return sum(i.lines.filter(function (l) { return prod(l.pid).cat === c.id; }), function (l) { return l.qty * l.price; }); }) }; }).filter(function (c) { return c.v > 0; }).sort(function (a, b) { return b.v - a.v; });
    var credit = state.customers.filter(function (c) { return custDue(c.id) >= c.limit * 0.6; });
    var hour = new Date().getHours(), greet = hour < 12 ? 'Good morning' : hour < 17 ? 'Good afternoon' : 'Good evening';
    var periods = [['1', 'Today'], ['7', '7 days'], ['30', '30 days']];
    var attn = '';
    if (w.length) attn += '<a href="#/orders" class="attn bad"><span class="ico">' + icon('orders') + '</span><span class="grow"><b>' + w.length + ' orders waiting for an invoice</b><small>' + money(sum(w, function (o) { return linesTotal(o.lines); })) + '</small></span><span class="cta">Invoice &rarr;</span></a>';
    if (credit.length) attn += '<a href="#/customers" class="attn warn"><span class="ico">' + icon('customers') + '</span><span class="grow"><b>' + credit.length + ' customers near their credit limit</b><small>' + esc(credit.slice(0, 2).map(function (c) { return c.name; }).join(', ')) + (credit.length > 2 ? ' and ' + (credit.length - 2) + ' more' : '') + '</small></span><span class="cta">Review &rarr;</span></a>';
    if (over30.length) attn += '<a href="#/invoices" class="attn neutral"><span class="ico">' + icon('collections') + '</span><span class="grow"><b>' + over30.length + ' invoices over 30 days late</b><small>' + money(sum(over30, invDue)) + ' in total</small></span><span class="cta">Collect &rarr;</span></a>';
    attn += '<a href="#/orders/new" class="attn good"><span class="ico">' + icon('clock') + '</span><span class="grow"><b>' + (windowStatus().open ? 'Order window is open' : 'Order desk is closed') + '</b><small>' + windowStatus().text + '</small></span><span class="cta">New order &rarr;</span></a>';
    return head(greet + ', ' + esc(me().name.split(' ')[0]), new Date().toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long' }) + ' <span class="pill good" style="margin-left:8px">' + icon('clock', 14) + windowStatus().text + '</span>',
      '<div class="seg" role="group" aria-label="Period">' + periods.map(function (x) { return '<button class="' + (p === x[0] ? 'on' : '') + '" data-act="period" data-v="' + x[0] + '">' + x[1] + '</button>'; }).join('') + '</div><a class="btn btn-p" href="#/orders/new">' + icon('plus', 18, 2.2) + 'New order</a>') +
      '<section class="kpis" aria-label="Key figures">' +
      '<div class="card butter kpi hero"><div class="lbl">Sales billed · ' + periods.filter(function (x) { return x[0] === p; })[0][1] + '</div><div class="row wrap" style="align-items:baseline"><div class="v">' + money(s.billed) + '</div>' + delta(s.billed, ps.billed) + '</div></div>' +
      '<div class="card kpi"><div class="lbl">Collected</div><div class="v">' + money(s.collected) + '</div><div class="meter good"><i style="width:' + rate + '%"></i></div><div class="lbl">' + rate + '% of billed ' + delta(s.collected, ps.collected) + '</div></div>' +
      '<div class="card kpi"><div class="lbl">Outstanding due</div><div class="v">' + money(out) + '</div><div class="lbl">' + Object.keys(owing).length + ' customers owe money</div>' + (over30.length ? '<a class="link bad" href="#/invoices">' + over30.length + ' invoices over 30 days late &rarr;</a>' : '') + '</div>' +
      '<div class="card kpi alert"><div class="lbl">Orders waiting for invoice</div><div class="v">' + w.length + '</div><div class="lbl">' + (w.length ? money(sum(w, function (o) { return linesTotal(o.lines); })) : 'All caught up') + '</div>' + (w.length ? '<a class="btn btn-p btn-s" style="align-self:flex-start" href="#/orders">Invoice them</a>' : '') + '</div></section>' +
      '<section class="row top wrap" style="gap:20px"><div class="card" style="min-width:0;flex:2 1 560px"><div class="row between wrap"><div><div class="ct">Billed vs collected</div><div class="lbl">Daily, last ' + span + ' days</div></div><div class="legend"><span><i class="key-line" style="background:#E5412D"></i>Billed</span><span><i class="key-line" style="background:#1F9E96"></i>Collected</span><a class="link" href="#/reports">Full report &rarr;</a></div></div>' + lineChart(days) + '</div>' +
      '<div class="card" style="flex:1 1 320px"><div class="ct" style="margin-bottom:8px">Needs your attention</div>' + attn + '</div></section>' +
      '<section class="row top wrap" style="gap:20px"><div class="card" style="flex:1 1 320px"><div class="ct">Top customers</div><div class="lbl" style="margin-bottom:14px">By billed amount</div>' +
      (top.length ? top.map(function (c) { return '<div class="bar-row" style="margin-bottom:14px"><div class="top-line"><span>' + esc(c.name) + '</span><span class="num strong">' + money(c.v) + '</span></div><div class="meter thick violet"><i style="width:' + Math.round(c.v / maxTop * 100) + '%"></i></div></div>'; }).join('') : '<div class="muted">No sales in this period.</div>') + '</div>' +
      '<div class="card" style="flex:1 1 320px"><div class="ct">Sales by category</div><div class="lbl" style="margin-bottom:14px">Share of billed amount</div><div class="row wrap" style="gap:20px">' + donut(byCat) + '<div class="col grow" style="min-width:140px">' + byCat.map(function (c) { return '<div class="row between"><span class="row" style="gap:8px"><i class="dot" style="background:' + c.color + '"></i>' + c.name + '</span><span class="num strong">' + Math.round(c.v / (sum(byCat, function (x) { return x.v; }) || 1) * 100) + '%</span></div>'; }).join('') + '</div></div></div></section>';
  }

  function pReports() {
    var s = stats(isoOf(29), today());
    var prods = state.products.map(function (p) { return { p: p, q: sum(s.inv, function (i) { return sum(i.lines.filter(function (l) { return l.pid === p.id; }), function (l) { return l.qty; }); }) }; }).sort(function (a, b) { return b.q - a.q; }).slice(0, 6);
    var maxQ = prods[0].q || 1;
    var staff = D.STAFF.map(function (n) { return { n: n, v: sum(s.inv.filter(function (i) { return i.staff === n; }), invTotal) }; }).sort(function (a, b) { return b.v - a.v; });
    var maxS = staff[0].v || 1;
    var ag = [['Not yet 7 days', 0, 7], ['7 to 15 days', 7, 15], ['15 to 30 days', 15, 30], ['Over 30 days', 30, 9999]].map(function (b, i) { return { n: b[0], v: sum(state.invoices.filter(function (inv) { var a = ageDays(inv.date); return invDue(inv) > 0 && a >= b[1] && a < b[2]; }), invDue), color: ['#1F9E96', '#C98A12', '#E5412D', '#7F1628'][i] }; });
    var agT = sum(ag, function (a) { return a.v; }) || 1;
    var wk = [0, 0, 0, 0, 0, 0, 0]; s.inv.forEach(function (i) { wk[new Date(i.date + 'T00:00:00').getDay()] += invTotal(i); });
    var maxW = Math.max.apply(null, wk) || 1, names = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    var rate = s.billed ? Math.round(s.collected / s.billed * 100) : 0;
    return head('KPI report', 'Last 30 days at a glance.') +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Billed</div><div class="v">' + money(s.billed) + '</div></div><div class="card kpi small"><div class="lbl">Collected</div><div class="v">' + money(s.collected) + '</div><div class="meter good"><i style="width:' + rate + '%"></i></div></div><div class="card kpi small"><div class="lbl">Invoices</div><div class="v">' + s.inv.length + '</div></div><div class="card kpi small alert"><div class="lbl">Receivable</div><div class="v" style="color:#B3231C">' + money(outstanding()) + '</div></div></section>' +
      '<section class="row top wrap" style="gap:20px"><div class="card" style="flex:1 1 360px"><div class="ct">Best sellers</div><div class="lbl" style="margin-bottom:14px">Units sold</div>' + prods.map(function (x) { return '<div class="bar-row" style="margin-bottom:14px"><div class="top-line"><span>' + esc(x.p.name) + '</span><span class="num strong">' + x.q + '</span></div><div class="meter thick"><i style="width:' + Math.round(x.q / maxQ * 100) + '%"></i></div></div>'; }).join('') + '</div>' +
      '<div class="card" style="flex:1 1 360px"><div class="ct">Salesperson board</div><div class="lbl" style="margin-bottom:14px">Billed amount</div>' + staff.map(function (x, i) { return '<div class="bar-row" style="margin-bottom:14px"><div class="top-line"><span>' + (i + 1) + '. ' + x.n + '</span><span class="num strong">' + money(x.v) + '</span></div><div class="meter thick good"><i style="width:' + Math.round(x.v / maxS * 100) + '%"></i></div></div>'; }).join('') + '</div></section>' +
      '<section class="row top wrap" style="gap:20px"><div class="card" style="flex:1 1 360px"><div class="ct">Who owes how long</div><div class="lbl" style="margin-bottom:14px">Outstanding by invoice age</div><div class="stackbar" style="margin-bottom:14px">' + ag.map(function (a) { return '<i style="width:' + (a.v / agT * 100) + '%;background:' + a.color + '"></i>'; }).join('') + '</div>' + ag.map(function (a) { return '<div class="row between" style="margin-bottom:6px"><span class="row" style="gap:8px"><i class="dot" style="background:' + a.color + '"></i>' + a.n + '</span><span class="num strong">' + money(a.v) + '</span></div>'; }).join('') + '</div>' +
      '<div class="card" style="flex:1 1 360px"><div class="ct">Sales by weekday</div><div class="lbl" style="margin-bottom:14px">Which days sell best</div><div class="cols cols-h">' + wk.map(function (v) { return '<div class="g"><i style="height:' + Math.max(3, Math.round(v / maxW * 100)) + '%;background:#7B4BB8"></i></div>'; }).join('') + '</div><div class="xlabels">' + names.map(function (n) { return '<span>' + n + '</span>'; }).join('') + '</div></div></section>';
  }

  function pNewOrder() {
    var c = ui.cart, selected = c.custId ? cust(c.custId) : null, ws = windowStatus();
    var lines = Object.keys(c.lines).filter(function (k) { return c.lines[k] > 0; }).map(function (k) { var p = prod(+k); return { p: p, qty: c.lines[k] }; });
    var total = sum(lines, function (l) { return l.qty * l.p.price; }), units = sum(lines, function (l) { return l.qty; });
    var after = selected ? custDue(selected.id) + total : 0, over = selected && after > selected.limit;
    var use = selected ? Math.min(100, Math.round(custDue(selected.id) / (selected.limit || 1) * 100)) : 0;
    var custBlock = selected ?
      '<div class="selected-cust"><div class="avatar big">' + initials(selected.name) + '</div><div class="grow"><b>' + esc(selected.name) + '</b><div class="muted">' + esc(selected.owner) + ' · ' + esc(selected.area) + '</div></div><div><div class="lbl">Owes now</div><b class="num">' + money(custDue(selected.id)) + '</b><div class="muted mini">Limit ' + money(selected.limit) + ' (' + use + '% used)</div></div><button class="btn btn-s" data-act="change-cust">Change</button></div>' +
      (over ? '<div class="note bad" style="margin-top:12px"><b>Over the credit limit.</b> With this order ' + esc(selected.name) + ' would owe ' + money(after) + ' against a limit of ' + money(selected.limit) + '.<label class="check" style="margin-top:8px"><input type="checkbox" data-act="override" ' + (c.override ? 'checked' : '') + '> Allow this order anyway</label></div>' : '') :
      '<div class="field"><label for="custQ">Find customer</label><input id="custQ" type="search" placeholder="Search by shop or owner name" value="' + esc(ui.custQ) + '" autocomplete="off"></div><div class="cust-results" id="custResults">' + custResults() + '</div>';
    var chips = '<button class="chip ' + (ui.prodCat === 'all' ? 'on' : '') + '" data-act="prodcat" data-v="all">All</button>' + state.categories.map(function (x) { return '<button class="chip ' + (ui.prodCat === x.id ? 'on' : '') + '" data-act="prodcat" data-v="' + x.id + '">' + esc(x.name) + '</button>'; }).join('');
    var grid = state.products.filter(function (p) { return p.active !== false && (ui.prodCat === 'all' || p.cat === ui.prodCat); }).map(function (p) {
      var q = c.lines[p.id] || 0;
      return '<div class="product ' + (q ? 'has' : '') + '"><div class="row">' + thumb(p) + '<div class="grow"><b>' + esc(p.name) + '</b><div class="muted mini">' + esc(cat(p.cat).name) + '</div><div class="num strong">' + money(p.price) + '</div></div></div>' +
        '<div class="row stepper"><button class="step" data-act="step" data-id="' + p.id + '" data-d="-5" aria-label="Remove 5 ' + esc(p.name) + '">−</button><div class="qty num" aria-live="polite">' + q + '</div><button class="step" data-act="step" data-id="' + p.id + '" data-d="5" aria-label="Add 5 ' + esc(p.name) + '">+</button></div></div>';
    }).join('');
    var cartHtml = lines.length ? lines.map(function (l) { return '<div class="cart-line"><div class="grow"><b>' + esc(l.p.name) + '</b><div class="muted mini">' + l.qty + ' × ' + money(l.p.price) + '</div></div><b class="num">' + money(l.qty * l.p.price) + '</b><button class="rm" data-act="rm" data-id="' + l.p.id + '" aria-label="Remove ' + esc(l.p.name) + '">' + icon('trash', 16) + '</button></div>'; }).join('') : '<div class="empty-dash">Add products to start the order.</div>';
    var ready = selected && lines.length && ws.open && (!over || c.override);
    var why = !ws.open ? 'The order desk is closed.' : !selected ? 'Choose a customer first.' : !lines.length ? 'Add at least one product.' : over && !c.override ? 'Tick "Allow this order anyway" to go over the limit.' : '';
    return head('New order', 'Pick the customer, then tap + on the products they want. Items go up by 5.') +
      (ws.open ? '' : '<div class="banner bad"><span><b>The order desk is closed.</b> ' + ws.text + '.</span>' + (can('window') ? '<a class="btn btn-s" href="#/window">Change order window</a>' : '') + '</div>') +
      '<div class="flow top"><div class="col grow" style="gap:20px;flex:2 1 560px"><div class="card"><div class="row" style="margin-bottom:12px"><span class="step-no">1</span><div class="ct">Customer</div></div>' + custBlock + '</div>' +
      '<div class="card"><div class="row between wrap" style="margin-bottom:12px"><div class="row"><span class="step-no">2</span><div class="ct">Products</div></div><div class="chips">' + chips + '</div></div><div class="product-grid">' + grid + '</div></div></div>' +
      '<aside class="card cart"><div class="ct">Order summary</div><div>' + cartHtml + '</div><div class="row between"><span class="lbl">' + units + ' units</span><b class="num" style="font-size:22px">' + money(total) + '</b></div>' +
      '<button class="btn btn-p btn-lg btn-block ' + (ready ? '' : 'is-off') + '" data-act="place" ' + (ready ? '' : 'disabled') + '>Place order</button>' + (ready ? '' : '<div class="muted mini" style="text-align:center">' + why + '</div>') + '</aside></div>';
  }
  function custResults() {
    var q = ui.custQ.trim().toLowerCase();
    var list = state.customers.filter(function (c) { return !q || (c.name + ' ' + c.owner + ' ' + c.area).toLowerCase().indexOf(q) >= 0; }).slice(0, 6);
    return list.length ? list.map(function (c) { return '<button class="cust" data-act="pick-cust" data-id="' + c.id + '"><b>' + esc(c.name) + '</b><small>' + esc(c.owner) + ' · ' + esc(c.area) + '</small></button>'; }).join('') : '<div class="empty">No customer matches.</div>';
  }

  function pOrders() {
    var q = ui.ordQ.trim().toLowerCase(), all = waiting(), w = all.filter(function (o) { return !q || (o.no + ' ' + cust(o.custId).name).toLowerCase().indexOf(q) >= 0; }), mg = can('order.manage');
    var rows = w.map(function (o) {
      var c = cust(o.custId), t = linesTotal(o.lines);
      return '<div class="list-row">' + (mg ? '<input type="checkbox" data-act="pick-order" data-id="' + o.id + '" ' + (ui.picked[o.id] ? 'checked' : '') + ' aria-label="Select ' + o.no + '">' : '') + '<div class="grow"><b>' + esc(c.name) + '</b> <span class="faint">· ' + o.no + '</span><div class="muted mini">' + o.lines.map(function (l) { return l.qty + ' ' + esc(prod(l.pid).name); }).join(', ') + '</div></div><div class="faint mini">' + dt(o.date) + ' · ' + o.staff + '</div><b class="num">' + money(t) + '</b>' +
        (mg ? '<button class="btn btn-s" data-act="modal" data-v="order-edit" data-id="' + o.id + '">Edit</button><button class="btn btn-s" data-act="order-cancel" data-id="' + o.id + '">Cancel</button><button class="btn btn-s btn-p" data-act="invoice-one" data-id="' + o.id + '">Invoice</button>' : '') + '</div>';
    }).join('');
    var n = Object.keys(ui.picked).filter(function (k) { return ui.picked[k]; }).length, cancelled = state.orders.filter(function (o) { return o.status === 'cancelled'; }).length;
    return head('Orders to invoice', 'Edit, cancel or turn waiting orders into invoices.', (can('order.create') ? '<a class="btn btn-p" href="#/orders/new">' + icon('plus', 18, 2.2) + 'New order</a>' : '')) +
      '<div class="row between wrap" style="gap:12px"><input type="search" id="ordQ" class="search" placeholder="Search order or customer" value="' + esc(ui.ordQ) + '" aria-label="Search orders"><span class="muted mini">' + (cancelled ? cancelled + ' cancelled today' : '') + '</span></div>' +
      '<div class="card flush"><div class="list-head">' + (mg ? '<label class="check"><input type="checkbox" data-act="pick-all" ' + (w.length && n === w.length ? 'checked' : '') + '> Select all</label>' : '') + '<span class="grow"></span><span class="muted">' + w.length + ' waiting · ' + money(sum(w, function (o) { return linesTotal(o.lines); })) + '</span></div>' + (rows || '<div class="empty">' + (all.length ? 'No order matches.' : 'All caught up. Nothing is waiting for an invoice.') + '</div>') + '</div>' +
      (mg && w.length ? '<div class="actionbar"><span>' + (n ? n + ' selected' : 'Select orders to invoice together') + '</span><button class="btn btn-p ' + (n ? '' : 'is-off') + '" data-act="invoice-selected">Create ' + (n || '') + ' invoice' + (n === 1 ? '' : 's') + '</button></div>' : '');
  }
  function pillFor(s) { return s === 'paid' ? '<span class="pill good">Paid</span>' : s === 'partial' ? '<span class="pill warn">Partly paid</span>' : '<span class="pill bad">Unpaid</span>'; }

  function filteredInvoices() {
    var q = ui.invQ.trim().toLowerCase(), from = ui.invFrom, to = ui.invTo;
    if (ui.invDays !== '0' && !from && !to) from = isoOf(+ui.invDays - 1);
    return state.invoices.slice().sort(function (a, b) { return b.id - a.id; }).filter(function (i) { return (!from || i.date >= from) && (!to || i.date <= to) && (!q || (i.no + ' ' + cust(i.custId).name).toLowerCase().indexOf(q) >= 0); });
  }

  function pInvoices() {
    var base = filteredInvoices(), cnt = { all: base.length, unpaid: 0, partial: 0, paid: 0 }; base.forEach(function (i) { cnt[invStatus(i)]++; });
    var list = base.filter(function (i) { return ui.invStatus === 'all' || invStatus(i) === ui.invStatus; });
    var billed = sum(list, invTotal), paid = sum(list, invPaid), per = 10;
    ui.invPage = Math.min(ui.invPage, Math.max(1, Math.ceil(list.length / per)));
    var page = list.slice((ui.invPage - 1) * per, ui.invPage * per);
    var days = [['1', 'Today'], ['7', 'Last 7 days'], ['30', 'Last 30 days'], ['0', 'Any date']];
    var acts = '<button class="btn" data-act="export-invoices">' + icon('download', 18) + 'Export Excel</button>' + (can('invoice.collect') ? '<button class="btn btn-p" data-act="modal" data-v="collect-cust">' + icon('collections', 18) + 'Collect from customer</button>' : '');
    return head('Invoices', 'Collect a payment against a customer. It clears their oldest invoices first.', acts) +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Billed</div><div class="v">' + money(billed) + '</div></div><div class="card kpi small"><div class="lbl">Collected</div><div class="v">' + money(paid) + '</div><div class="meter good"><i style="width:' + (billed ? Math.min(100, Math.round(paid / billed * 100)) : 0) + '%"></i></div></div><div class="card kpi small alert"><div class="lbl">Still to collect</div><div class="v" style="color:#B3231C">' + money(sum(list, invDue)) + '</div></div></section>' +
      '<div class="row between wrap" style="gap:12px"><div class="seg" role="group" aria-label="Payment status">' + [['all', 'All'], ['unpaid', 'Unpaid'], ['partial', 'Partly paid'], ['paid', 'Paid']].map(function (x) { return '<button class="' + (ui.invStatus === x[0] ? 'on' : '') + '" data-act="inv-status" data-v="' + x[0] + '">' + x[1] + ' <span class="faint">' + cnt[x[0]] + '</span></button>'; }).join('') + '</div>' +
      '<div class="row wrap" style="gap:8px"><input type="search" id="invQ" placeholder="Search invoice or customer" value="' + esc(ui.invQ) + '" aria-label="Search invoices" style="width:220px">' +
      '<select id="invDays" aria-label="Period" style="width:auto">' + days.map(function (d) { return '<option value="' + d[0] + '"' + (ui.invDays === d[0] ? ' selected' : '') + '>' + d[1] + '</option>'; }).join('') + '</select>' +
      '<label class="row lbl" style="gap:6px">From <input type="date" id="invFrom" value="' + ui.invFrom + '" style="width:auto"></label><label class="row lbl" style="gap:6px">To <input type="date" id="invTo" value="' + ui.invTo + '" style="width:auto"></label></div></div>' +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Invoice</th><th>Date</th><th>Customer</th><th class="r">Total</th><th class="r">Paid</th><th class="r">Due</th><th>Status</th></tr></thead><tbody>' +
      (page.map(function (i) { return '<tr class="click" data-go="#/invoices/' + i.id + '"><td><a class="a-link" href="#/invoices/' + i.id + '">' + i.no + '</a></td><td>' + dt(i.date) + '</td><td>' + esc(cust(i.custId).name) + '</td><td class="r num">' + money(invTotal(i)) + '</td><td class="r num">' + money(invPaid(i)) + '</td><td class="r num strong">' + money(invDue(i)) + '</td><td>' + pillFor(invStatus(i)) + '</td></tr>'; }).join('') || '<tr><td colspan="7" class="empty">No invoices match.</td></tr>') +
      '</tbody></table></div>' + pager(ui.invPage, list.length, per, 'inv-page') + '</div>';
  }

  function pInvoice(id) {
    var inv = state.invoices.filter(function (i) { return i.id === id; })[0];
    if (!inv) return head('Invoice not found', '<a class="a-link" href="#/invoices">Back to invoices</a>');
    var c = cust(inv.custId), due = invDue(inv), cols = state.collections.filter(function (x) { return x.invId === inv.id; }), crs = state.returns.filter(function (r) { return r.invId === inv.id; });
    var overpaid = Math.max(0, invPaid(inv) - invTotal(inv));
    var sheet = '<div class="sheet"><div class="row between top"><div><h2>Invoice ' + inv.no + '</h2><div class="muted">' + dt(inv.date) + ' · Sales: ' + inv.staff + '</div></div><img src="img/logo-purple.jpg" alt="Talukder Foods" style="width:92px;border-radius:10px"></div>' +
      '<div><div class="sec-label">Billed to</div><b>' + esc(c.name) + '</b><div class="muted">' + esc(c.owner) + ' · ' + esc(c.area) + ' · ' + esc(c.phone) + '</div></div>' +
      '<table><thead><tr><th>Item</th><th class="r">Qty</th><th class="r">Price</th><th class="r">Amount</th></tr></thead><tbody>' + inv.lines.map(function (l) { return '<tr><td>' + esc(prod(l.pid).name) + '</td><td class="r">' + l.qty + '</td><td class="r">' + money(l.price) + '</td><td class="r">' + money(l.qty * l.price) + '</td></tr>'; }).join('') + '</tbody></table>' +
      '<div class="col" style="align-items:flex-end;gap:6px">' + (inv.discount ? '<div>Discount <b class="num">− ' + money(inv.discount) + '</b></div>' : '') + crs.map(function (r) { return '<div>Credit note ' + r.no + ' <b class="num">− ' + money(r.amount) + '</b></div>'; }).join('') + '<div>Total <b class="num" style="font-size:20px">' + money(invTotal(inv)) + '</b></div><div class="muted">Paid ' + money(invPaid(inv)) + '</div><div>Due <b class="num" style="font-size:20px;color:' + (due ? '#B3231C' : '#0B5F59') + '">' + money(due) + '</b></div>' + (overpaid ? '<div class="muted mini">Overpaid by ' + money(overpaid) + ' after the credit note. Refund or keep it against the next invoice.</div>' : '') + '</div></div>';
    var tools = '<div class="row wrap" style="gap:8px">' + (due > 0 && can('invoice.discount') ? '<button class="btn btn-s" data-act="modal" data-v="discount" data-id="' + inv.id + '">' + icon('tag', 16) + 'Give discount</button>' : '') + (can('invoice.return') ? '<a class="btn btn-s" href="#/invoices/' + inv.id + '/return">' + icon('undo', 16) + 'Return items</a>' : '') + '</div>';
    var panel = due > 0 && can('invoice.collect') ?
      '<aside class="card panel"><div class="ct">Collect payment</div><div class="lbl">Due on this invoice: <b class="num">' + money(due) + '</b></div><div class="field"><label for="amt">Amount received</label><div style="position:relative"><span style="position:absolute;left:12px;top:14px;font-size:22px;font-weight:600;color:var(--red)">৳</span><input id="amt" class="amount-input" type="number" min="1" max="' + due + '" value="' + due + '" inputmode="numeric"></div></div>' +
      '<div class="field"><span class="fl">Method</span><div class="seg" id="method">' + ['Cash', 'bKash', 'Bank'].map(function (m, k) { return '<button type="button" class="' + (k === 0 ? 'on' : '') + '" data-act="method" data-v="' + m + '">' + m + '</button>'; }).join('') + '</div></div>' +
      '<button class="btn btn-p btn-lg btn-block" data-act="collect" data-id="' + inv.id + '">Record payment</button>' + tools + '</aside>' :
      '<aside class="card panel">' + (due > 0 ? '<div class="note grey">You can view this invoice but your role cannot collect payments.</div>' : '<div class="banner"><b>Fully paid</b></div>') + (cols.length ? '<div class="sec-label">Payments</div>' + cols.map(function (x) { return '<div class="row between"><span>' + dt(x.date) + ' · ' + x.method + '</span><b class="num">' + money(x.amount) + '</b></div>'; }).join('') : '') + tools + '</aside>';
    return head('Invoice ' + inv.no, '<a class="a-link" href="#/invoices">&larr; All invoices</a>', '<button class="btn" data-act="print">' + icon('print', 18) + 'Print</button>') + '<div class="flow top"><div class="grow" style="flex:2 1 560px">' + sheet + '</div>' + panel + '</div>';
  }

  function returnedQty(inv, pid) { return sum(state.returns.filter(function (r) { return r.invId === inv.id; }), function (r) { return sum(r.lines.filter(function (l) { return l.pid === pid; }), function (l) { return l.qty; }); }); }
  function pReturn(id) {
    var inv = state.invoices.filter(function (i) { return i.id === id; })[0];
    if (!inv) return head('Invoice not found', '<a class="a-link" href="#/invoices">Back to invoices</a>');
    var rows = inv.lines.map(function (l) { var left = l.qty - returnedQty(inv, l.pid); return '<tr><td>' + esc(prod(l.pid).name) + '</td><td class="r">' + l.qty + '</td><td class="r">' + (l.qty - left) + '</td><td class="r">' + money(l.price) + '</td><td class="r"><input type="number" class="rq" min="0" max="' + left + '" value="0" data-pid="' + l.pid + '" data-price="' + l.price + '" aria-label="Return quantity for ' + esc(prod(l.pid).name) + '" style="width:90px;text-align:right"' + (left ? '' : ' disabled') + '></td></tr>'; }).join('');
    return head('Return items', 'Invoice ' + inv.no + ' · ' + esc(cust(inv.custId).name) + ' · <a class="a-link" href="#/invoices/' + inv.id + '">Back to invoice</a>') +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Item</th><th class="r">Billed</th><th class="r">Returned</th><th class="r">Price</th><th class="r">Return now</th></tr></thead><tbody>' + rows + '</tbody></table></div></div>' +
      '<div class="actionbar"><span>Credit note total: <b class="num" id="creditTotal">৳ 0</b></span><button class="btn btn-p" data-act="return-submit" data-id="' + inv.id + '">Create credit note</button></div>';
  }

  function pCollections() {
    var list = state.collections.slice().sort(function (a, b) { return b.id - a.id; }), per = 10;
    ui.colPage = Math.min(ui.colPage, Math.max(1, Math.ceil(list.length / per)));
    var page = list.slice((ui.colPage - 1) * per, ui.colPage * per), t = isoOf(0);
    var todays = sum(list.filter(function (c) { return c.date === t; }), function (c) { return c.amount; });
    return head('Collections', 'Every payment received, newest first.', '<button class="btn" data-act="export-collections">' + icon('download', 18) + 'Export Excel</button>') +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Collected today</div><div class="v">' + money(todays) + '</div></div><div class="card kpi small"><div class="lbl">Last 30 days</div><div class="v">' + money(sum(list, function (c) { return c.amount; })) + '</div></div><div class="card kpi small"><div class="lbl">Payments</div><div class="v">' + list.length + '</div></div></section>' +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Date</th><th>Customer</th><th>Invoice</th><th>Method</th><th class="r">Amount</th></tr></thead><tbody>' +
      page.map(function (c) { var i = state.invoices.filter(function (x) { return x.id === c.invId; })[0]; return '<tr><td>' + dt(c.date) + '</td><td>' + esc(cust(c.custId).name) + '</td><td><a class="a-link" href="#/invoices/' + c.invId + '">' + (i ? i.no : '') + '</a></td><td><span class="pill neutral">' + c.method + '</span></td><td class="r num strong">' + money(c.amount) + '</td></tr>'; }).join('') +
      '</tbody></table></div>' + pager(ui.colPage, list.length, per, 'col-page') + '</div>';
  }

  function pProducts() {
    var q = ui.prodQ.trim().toLowerCase(), mg = can('products');
    var list = state.products.filter(function (p) { return (ui.prodCat === 'all' || p.cat === ui.prodCat) && (!q || p.name.toLowerCase().indexOf(q) >= 0); });
    var act = '<button class="btn" data-act="export-products">' + icon('download', 18) + 'Export Excel</button>' + (mg ? '<button class="btn btn-p" data-act="modal" data-v="product">' + icon('plus', 18, 2.2) + 'Add product</button>' : '');
    var chips = '<button class="chip ' + (ui.prodCat === 'all' ? 'on' : '') + '" data-act="prodcat" data-v="all">All</button>' + state.categories.map(function (x) { return '<button class="chip ' + (ui.prodCat === x.id ? 'on' : '') + '" data-act="prodcat" data-v="' + x.id + '">' + esc(x.name) + '</button>'; }).join('');
    return head('Products', state.products.length + ' items in the range.', act) +
      '<div class="row between wrap" style="gap:12px"><div class="chips">' + chips + '</div><input type="search" id="prodQ" class="search" placeholder="Search products" value="' + esc(ui.prodQ) + '" aria-label="Search products"></div>' +
      '<div class="product-grid">' + (list.length ? list.map(function (p) {
        var c = cat(p.cat), sold = sum(state.invoices, function (i) { return sum(i.lines.filter(function (l) { return l.pid === p.id; }), function (l) { return l.qty; }); }), on = p.active !== false;
        return '<div class="product" style="' + (on ? '' : 'opacity:.6') + '"><div class="row">' + thumb(p) + '<div class="grow"><b>' + esc(p.name) + '</b><div><span class="pill neutral">' + esc(c.name) + '</span> ' + (on ? '' : '<span class="pill warn">Hidden</span>') + '</div></div></div>' +
          '<div class="row between"><span class="num strong" style="font-size:18px">' + money(p.price) + '</span><span class="muted mini">' + sold + ' sold in 30 days</span></div>' +
          (mg ? '<div class="row"><button class="btn btn-s grow" data-act="modal" data-v="product" data-id="' + p.id + '">Edit</button><button class="btn btn-s grow" data-act="prod-toggle" data-id="' + p.id + '">' + (on ? 'Hide' : 'Show') + '</button></div>' : '') + '</div>';
      }).join('') : '<div class="card empty" style="grid-column:1/-1">No product matches.</div>') + '</div>';
  }

  var CAT_CLS = ['cat-1', 'cat-3', 'cat-4', 'cat-2', 'cat-0'], CAT_COL = ['#E5412D', '#C98A12', '#1F9E96', '#7B4BB8', '#8F8294', '#2F6FD6', '#C2417C'];
  function pCategories() {
    var rows = state.categories.map(function (c) {
      var n = state.products.filter(function (p) { return p.cat === c.id; }).length;
      return '<tr><td><span class="row" style="gap:10px"><i class="dot" style="background:' + c.color + '"></i><b>' + esc(c.name) + '</b></span></td><td class="r num">' + n + '</td><td class="r"><button class="btn btn-s" data-act="modal" data-v="category" data-id="' + (state.categories.indexOf(c) + 1) + '">Rename</button> ' + (n ? '<span class="faint mini">In use</span>' : '<button class="btn btn-s" data-act="cat-delete" data-cid="' + c.id + '">Delete</button>') + '</td></tr>';
    }).join('');
    return head('Categories', 'Product types. They group the menu, the production plan and the reports.', '<button class="btn btn-p" data-act="modal" data-v="category">' + icon('plus', 18, 2.2) + 'Add category</button>') +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Category</th><th class="r">Products</th><th></th></tr></thead><tbody>' + rows + '</tbody></table></div></div>';
  }

  function pCustomers() {
    var q = ui.custFind.trim().toLowerCase(), per = 8;
    var list = state.customers.filter(function (c) { return !q || (c.name + ' ' + c.owner + ' ' + c.area).toLowerCase().indexOf(q) >= 0; });
    ui.custPage = Math.min(ui.custPage, Math.max(1, Math.ceil(list.length / per)));
    var page = list.slice((ui.custPage - 1) * per, ui.custPage * per);
    var rows = page.map(function (c) { var due = custDue(c.id), use = Math.min(100, Math.round(due / (c.limit || 1) * 100)); return '<tr class="click" data-go="#/customers/' + c.id + '"><td><div class="row"><div class="avatar-s">' + initials(c.name) + '</div><div><b>' + esc(c.name) + '</b><div class="muted mini">' + esc(c.owner) + '</div></div></div></td><td>' + esc(c.area) + '</td><td class="r num">' + money(custBilled(c.id)) + '</td><td class="r num strong">' + money(due) + '</td><td style="min-width:150px"><div class="meter ' + (use >= 80 ? '' : 'good') + '"><i style="width:' + use + '%;' + (use >= 80 ? 'background:#E5412D' : '') + '"></i></div><div class="muted mini">' + use + '% of ' + money(c.limit) + '</div></td></tr>'; }).join('');
    return head('Customers', 'Shops you sell to, what they owe and how close they are to their credit limit.', '<button class="btn" data-act="export-customers">' + icon('download', 18) + 'Export Excel</button><button class="btn btn-p" data-act="modal" data-v="customer">' + icon('plus', 18, 2.2) + 'Add customer</button>') +
      '<div class="row between wrap"><input type="search" id="custFind" class="search" placeholder="Search shop, owner or area" value="' + esc(ui.custFind) + '" aria-label="Search customers"><span class="muted mini">' + list.length + ' customers</span></div>' +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Customer</th><th>Area</th><th class="r">Billed (30d)</th><th class="r">Owes</th><th>Credit used</th></tr></thead><tbody>' + (rows || '<tr><td colspan="5" class="empty">No customer matches.</td></tr>') + '</tbody></table></div>' + pager(ui.custPage, list.length, per, 'cust-page') + '</div>';
  }

  function statementRows(id) {
    var c = cust(id), ev = [];
    state.invoices.filter(function (i) { return i.custId === id; }).forEach(function (i) {
      ev.push({ date: i.date, o: 1, ref: i.no, what: 'Invoice', debit: linesTotal(i.lines) - (i.discount || 0), credit: 0, link: '#/invoices/' + i.id });
      state.returns.filter(function (r) { return r.invId === i.id; }).forEach(function (r) { ev.push({ date: r.date, o: 2, ref: r.no, what: 'Credit note for ' + i.no, debit: 0, credit: r.amount }); });
    });
    state.collections.filter(function (x) { return x.custId === id; }).forEach(function (x) { var i = state.invoices.filter(function (y) { return y.id === x.invId; })[0]; ev.push({ date: x.date, o: 3, ref: 'PAY-' + x.id, what: 'Payment (' + x.method + ')' + (i ? ' for ' + i.no : ''), debit: 0, credit: x.amount }); });
    ev.sort(function (a, b) { return a.date < b.date ? -1 : a.date > b.date ? 1 : a.o - b.o; });
    var bal = 0, open = 0, out = [];
    ev.forEach(function (e) { bal += e.debit - e.credit; e.bal = bal; if (ui.stFrom && e.date < ui.stFrom) { open = bal; return; } if (ui.stTo && e.date > ui.stTo) return; out.push(e); });
    return { rows: out, opening: ui.stFrom ? open : 0, closing: out.length ? out[out.length - 1].bal : open, c: c };
  }
  function pCustomer(id) {
    var c = cust(id); if (!c) return head('Customer not found', '<a class="a-link" href="#/customers">All customers</a>');
    var st = statementRows(id), due = custDue(id), invCount = state.invoices.filter(function (i) { return i.custId === id; }).length;
    var acts = (can('invoice.collect') && due > 0 ? '<button class="btn btn-p" data-act="modal" data-v="collect-cust" data-id="' + id + '">' + icon('collections', 18) + 'Collect payment</button>' : '') + (can('order.create') ? '<a class="btn" data-act="order-for" data-id="' + id + '">' + icon('plus', 18, 2.2) + 'New order</a>' : '') + '<button class="btn" data-act="modal" data-v="customer" data-id="' + id + '">Edit</button>' + (invCount ? '' : '<button class="btn" data-act="cust-delete" data-id="' + id + '">Delete</button>');
    return head(esc(c.name), esc(c.owner) + ' · ' + esc(c.area) + ' · ' + esc(c.phone) + ' · <a class="a-link" href="#/customers">All customers</a>', acts) +
      (due >= c.limit ? '<div class="banner bad"><span><b>At or over the credit limit.</b> ' + esc(c.name) + ' owes ' + money(due) + ' against a limit of ' + money(c.limit) + '.</span></div>' : '') +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Billed (30 days)</div><div class="v">' + money(custBilled(id)) + '</div></div><div class="card kpi small alert"><div class="lbl">Owes now</div><div class="v" style="color:#B3231C">' + money(due) + '</div></div><div class="card kpi small"><div class="lbl">Credit limit</div><div class="v">' + money(c.limit) + '</div></div></section>' +
      '<div class="card flush"><div class="row between wrap" style="padding:18px 20px 10px;gap:12px"><div class="ct">Statement</div><div class="row wrap" style="gap:8px"><label class="row lbl" style="gap:6px">From <input type="date" id="stFrom" value="' + ui.stFrom + '" style="width:auto"></label><label class="row lbl" style="gap:6px">To <input type="date" id="stTo" value="' + ui.stTo + '" style="width:auto"></label><button class="btn btn-s" data-act="st-clear">Clear</button><button class="btn btn-s" data-act="export-statement" data-id="' + id + '">' + icon('download', 16) + 'Excel</button><button class="btn btn-s" data-act="print">' + icon('print', 16) + 'Print</button></div></div>' +
      '<div class="tbl-wrap"><table class="tbl"><thead><tr><th>Date</th><th>Reference</th><th>Details</th><th class="r">Billed</th><th class="r">Paid or credited</th><th class="r">Balance</th></tr></thead><tbody>' +
      (ui.stFrom ? '<tr><td colspan="5"><b>Opening balance</b></td><td class="r num strong">' + money(st.opening) + '</td></tr>' : '') +
      st.rows.map(function (e) { return '<tr><td>' + dt(e.date) + '</td><td>' + (e.link ? '<a class="a-link" href="' + e.link + '">' + e.ref + '</a>' : e.ref) + '</td><td>' + esc(e.what) + '</td><td class="r num">' + (e.debit ? money(e.debit) : '') + '</td><td class="r num">' + (e.credit ? money(e.credit) : '') + '</td><td class="r num strong">' + money(e.bal) + '</td></tr>'; }).join('') +
      '<tr><td colspan="5"><b>Closing balance</b></td><td class="r num strong" style="font-size:16px">' + money(st.closing) + '</td></tr></tbody></table></div></div>';
  }

  function shiftDay(iso, n) { var d = new Date(iso + 'T00:00:00'); d.setDate(d.getDate() + n); return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0'); }

  function pProduction() {
    var day = ui.planDate || today();
    // Orders for the day = invoices raised that day plus orders still waiting for an invoice.
    var src = state.invoices.filter(function (i) { return i.date === day; }).concat(state.orders.filter(function (o) { return o.status === 'waiting' && o.date === day; }));
    var qty = {}, custs = {};
    src.forEach(function (o) { custs[o.custId] = 1; o.lines.forEach(function (l) { qty[l.pid] = (qty[l.pid] || 0) + l.qty; }); });
    var groups = state.categories.map(function (c, ci) {
      var items = state.products.filter(function (p) { return p.cat === c.id && qty[p.id]; }).map(function (p) { return { name: p.name, q: qty[p.id] }; }).sort(function (a, b) { return b.q - a.q; });
      return { name: c.name, color: c.color, total: sum(items, function (x) { return x.q; }), items: items };
    }).filter(function (g) { return g.total > 0; }).sort(function (a, b) { return b.total - a.total; });
    var total = sum(groups, function (g) { return g.total; }), max = Math.max.apply(null, groups.map(function (g) { return g.items[0].q; }).concat([1]));
    var live = day === today(), nProd = sum(groups, function (g) { return g.items.length; });
    var nav = '<div class="row" style="gap:4px;padding:4px;border-radius:12px;background:#fff;border:1px solid var(--line)"><button class="daynav" data-act="plan-day" data-d="-1" aria-label="Previous day">&lsaquo;</button><input type="date" id="planDate" value="' + day + '" aria-label="Date" style="border:0;background:transparent;min-height:40px;width:auto;font-weight:600"><button class="daynav" data-act="plan-day" data-d="1" aria-label="Next day">&rsaquo;</button></div>';
    return head('Production plan', 'What to make, from the orders received for the day', nav + '<button class="btn" data-act="export-production">' + icon('download', 18) + 'Excel</button><button class="btn" data-act="print">' + icon('print', 18) + 'Print sheet</button>') +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Packs to make</div><div class="v">' + total.toLocaleString('en-IN') + '</div>' + (live ? '<span class="pill good live" style="align-self:flex-start"><i class="dot"></i>Live · orders still arriving</span>' : '') + '</div>' +
      '<div class="card kpi small"><div class="lbl">Orders</div><div class="v">' + src.length + '</div><div class="lbl">From ' + Object.keys(custs).length + ' customers</div></div>' +
      '<div class="card kpi small"><div class="lbl">Products</div><div class="v">' + nProd + '</div><div class="lbl">in ' + groups.length + ' categories</div></div>' +
      '<div class="card kpi small butter"><div class="lbl">Order desk closes</div><div class="v" style="color:var(--maroon)">' + windowStatus().close + '</div><div class="lbl">' + (live ? (windowStatus().open ? 'Orders are open' : 'Orders are closed') : 'Past day') + '</div></div></section>' +
      '<div class="row top wrap" style="gap:22px"><section class="col grow" style="flex:2 1 560px;min-width:0;gap:18px">' +
      (groups.length ? groups.map(function (g) { return '<div class="card flush"><div class="list-head"><span class="row" style="gap:10px"><i class="dot" style="background:' + g.color + '"></i><span class="ct">' + g.name + '</span></span><span class="grow"></span><span class="num strong">' + g.total.toLocaleString('en-IN') + ' packs</span></div>' + g.items.map(function (x) { return '<div class="list-row"><div style="width:200px;flex:none;font-weight:500">' + esc(x.name) + '</div><div class="meter thick violet grow"><i style="width:' + Math.round(x.q / max * 100) + '%;background:' + g.color + '"></i></div><div class="num strong" style="width:70px;text-align:right">' + x.q + '</div></div>'; }).join('') + '</div>'; }).join('') : '<div class="card empty">No orders for this day.</div>') +
      '</section><aside class="card panel"><div><div class="ct">Share by category</div><div class="lbl">Of ' + total.toLocaleString('en-IN') + ' packs</div></div>' +
      (groups.length ? '<div class="stackbar">' + groups.map(function (g) { return '<i style="flex:' + g.total + ';background:' + g.color + '"></i>'; }).join('') + '</div>' + groups.map(function (g) { return '<div class="row between"><span class="row" style="gap:8px"><i class="dot" style="background:' + g.color + '"></i>' + g.name + '</span><span><span class="num strong">' + g.total.toLocaleString('en-IN') + '</span> <span class="lbl">· ' + Math.round(g.total / total * 100) + '%</span></span></div>'; }).join('') : '') + '</aside></div>';
  }

  function pUsers() {
    var rows = state.users.map(function (u) {
      var self = me().id === u.id;
      return '<tr><td><div class="row"><div class="avatar-s">' + initials(u.name) + '</div><div><b>' + esc(u.name) + '</b><div class="muted mini">@' + esc(u.username) + '</div></div></div></td><td><span class="pill info">' + esc(u.role) + '</span></td><td>' + (u.active ? '<span class="pill good">Active</span>' : '<span class="pill neutral">Disabled</span>') + '</td><td class="r">' + (self ? '<span class="faint mini">You</span> <button class="btn btn-s" data-act="modal" data-v="password" data-id="' + u.id + '">Change password</button>' : '<button class="btn btn-s" data-act="signin-as" data-id="' + u.id + '"' + (u.active ? '' : ' disabled') + '>Sign in as</button> <button class="btn btn-s" data-act="modal" data-v="user" data-id="' + u.id + '">Edit</button> <button class="btn btn-s" data-act="modal" data-v="password" data-id="' + u.id + '">Reset password</button> <button class="btn btn-s" data-act="user-toggle" data-id="' + u.id + '">' + (u.active ? 'Disable' : 'Enable') + '</button>') + '</td></tr>';
    }).join('');
    return head('Users', 'People who can sign in. Roles decide what each person can see and do.', '<button class="btn btn-p" data-act="modal" data-v="user">' + icon('plus', 18, 2.2) + 'Add user</button>') +
      '<div class="note">Tip: press <b>Sign in as</b> on a salesperson or accountant to see the app the way they see it. Menus and pages follow the role.</div>' +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Name</th><th>Role</th><th>Status</th><th></th></tr></thead><tbody>' + rows + '</tbody></table></div></div>';
  }

  // ---------- notices, notifications, roles, order window, audit ----------
  function pNotices() {
    var list = state.notices.slice().sort(function (a, b) { return b.id - a.id; });
    return head('Notices', 'Announcements from the office. Newest first.', can('notices') ? '<button class="btn btn-p" data-act="modal" data-v="notice">' + icon('plus', 18, 2.2) + 'Post notice</button>' : '') +
      '<div class="row top wrap" style="gap:20px"><div class="col grow" style="flex:2 1 520px;gap:14px">' + (list.length ? list.map(function (n) {
        return '<div class="card"><div class="row between top wrap"><div><div class="ct" style="font-size:18px;color:var(--maroon)">' + esc(n.title) + '</div><div class="lbl">' + dt(n.date) + ' · ' + esc(n.by) + '</div></div>' + (can('notices') ? '<button class="btn btn-s" data-act="notice-delete" data-id="' + n.id + '" aria-label="Delete notice ' + esc(n.title) + '">' + icon('trash', 16) + 'Remove</button>' : '') + '</div><p style="margin:12px 0 0;color:var(--ink-2)">' + esc(n.body) + '</p></div>';
      }).join('') : '<div class="card empty">No notices right now.</div>') + '</div>' +
      '<aside class="card panel"><div class="ct">Today</div>' + (can('order.create') ? '<a class="attn good" href="#/orders/new"><span class="ico">' + icon('new-order') + '</span><span class="grow"><b>Take an order</b><small>' + windowStatus().text + '</small></span><span class="cta">Start &rarr;</span></a>' : '') + (can('dashboard') ? '<a class="attn neutral" href="#/dashboard"><span class="ico">' + icon('dashboard') + '</span><span class="grow"><b>Open the dashboard</b><small>Sales, collections and what needs attention</small></span><span class="cta">Open &rarr;</span></a>' : '') + '<a class="attn warn" href="#/notifications"><span class="ico">' + icon('bell') + '</span><span class="grow"><b>' + state.notifications.filter(function (n) { return !n.read; }).length + ' unread notifications</b><small>Orders, payments and reminders</small></span><span class="cta">View &rarr;</span></a></aside></div>';
  }

  function pNotifications() {
    var list = state.notifications.slice().sort(function (a, b) { return a.time < b.time ? 1 : -1; }).filter(function (n) { return ui.noteFilter === 'all' || !n.read; }).slice(0, 40), unread = state.notifications.filter(function (n) { return !n.read; }).length;
    return head('Notifications', unread ? unread + ' unread' : 'You are all caught up.', '<button class="btn" data-act="note-all"' + (unread ? '' : ' disabled') + '>Mark all as read</button>') +
      '<div class="seg" role="group" aria-label="Filter"><button class="' + (ui.noteFilter === 'all' ? 'on' : '') + '" data-act="note-filter" data-v="all">All</button><button class="' + (ui.noteFilter === 'unread' ? 'on' : '') + '" data-act="note-filter" data-v="unread">Unread <span class="faint">' + unread + '</span></button></div>' +
      '<div class="card flush">' + (list.length ? list.map(function (n) { return '<div class="list-row ' + (n.read ? '' : 'focus') + '"><i class="dot round" style="background:' + (n.read ? 'transparent' : '#E5412D') + '"></i><div class="grow"><b>' + esc(n.title) + '</b><div class="muted mini">' + esc(n.body) + '</div></div><span class="faint mini">' + dtt(n.time) + '</span>' + (n.read ? '' : '<button class="btn btn-s" data-act="note-read" data-id="' + n.id + '">Mark read</button>') + '</div>'; }).join('') : '<div class="empty">Nothing here.</div>') + '</div>';
  }

  function pRoles() {
    var sel = state.roles.filter(function (r) { return r.id === ui.roleSel; })[0] || state.roles[0], groups = {};
    D.PERMS.forEach(function (p) { (groups[p[2]] = groups[p[2]] || []).push(p); });
    var users = function (r) { return state.users.filter(function (u) { return u.role === r.name; }).length; };
    var left = state.roles.map(function (r) { return '<button class="list-row ' + (r.id === sel.id ? 'focus' : '') + '" style="width:100%;border:0;background:' + (r.id === sel.id ? 'var(--butter-tint)' : '#fff') + ';text-align:left" data-act="role-sel" data-id="' + r.id + '"><div class="grow"><b>' + esc(r.name) + '</b><div class="muted mini">' + users(r) + ' user' + (users(r) === 1 ? '' : 's') + ' · ' + (r.locked ? 'all permissions' : r.perms.length + ' of ' + D.PERMS.length + ' permissions') + '</div></div>' + (r.locked ? icon('lock', 16) : '') + '</button>'; }).join('');
    var right = Object.keys(groups).map(function (g) {
      return '<div class="card"><div class="sec-label" style="margin-bottom:6px">' + g + '</div>' + groups[g].map(function (p) {
        var on = sel.locked || sel.perms.indexOf(p[0]) >= 0;
        return '<label class="perm-row"><input type="checkbox" class="swc" data-act="perm-toggle" data-k="' + p[0] + '" ' + (on ? 'checked' : '') + (sel.locked ? ' disabled' : '') + '><span class="grow">' + p[1] + '</span></label>';
      }).join('') + '</div>';
    }).join('');
    return head('Roles', 'Decide what each role can see and do. Changes apply to everyone with that role.', '<button class="btn btn-p" data-act="modal" data-v="role">' + icon('plus', 18, 2.2) + 'Add role</button>') +
      '<div class="row top wrap" style="gap:20px"><div class="col" style="flex:1 1 280px;max-width:360px;gap:12px"><div class="card flush">' + left + '</div>' + (sel.locked ? '<div class="note grey">Administrator always has every permission.</div>' : (users(sel) ? '<div class="note grey">Move its users to another role before deleting this one.</div>' : '<button class="btn" data-act="role-delete" data-id="' + sel.id + '">Delete role</button>')) + '</div>' +
      '<div class="col grow" style="flex:2 1 460px;gap:14px"><div class="ct" style="font-size:20px">' + esc(sel.name) + '</div>' + right + '</div></div>';
  }

  function pWindow() {
    var w = state.window, ws = windowStatus(), toMin = function (t) { var p = t.split(':'); return +p[0] * 60 + +p[1]; }, d = new Date(), nowM = d.getHours() * 60 + d.getMinutes();
    var tl = w.enforce ? '<div class="win" style="left:' + toMin(w.open) / 14.4 + '%;width:' + Math.max(0, toMin(w.close) - toMin(w.open)) / 14.4 + '%">' + fmtTime(w.open) + ' to ' + fmtTime(w.close) + '</div>' : '<div class="win" style="left:0;width:100%">Open all day</div>';
    var todays = state.orders.filter(function (o) { return o.date === today(); }).length + state.invoices.filter(function (i) { return i.date === today(); }).length;
    return head('Order window', 'The hours when orders can be taken. Outside them the New order page is closed.') +
      '<div class="row top wrap" style="gap:20px"><div class="card grow" style="flex:2 1 460px"><div class="row between wrap"><div><div class="ct">Today</div><div class="lbl">' + ws.text + '</div></div><span class="pill ' + (ws.open ? 'good live' : 'bad') + '">' + (ws.open ? '<i class="dot"></i>Open now' : 'Closed now') + '</span></div>' +
      '<div class="timeline" style="margin:18px 0 6px">' + tl + '<div class="now" style="left:' + nowM / 14.4 + '%"></div></div><div class="xlabels"><span>12 AM</span><span>6 AM</span><span>12 PM</span><span>6 PM</span><span>12 AM</span></div>' +
      '<div class="muted mini" style="margin-top:10px">The dark marker is the time now. ' + todays + ' orders and invoices so far today.</div></div>' +
      '<form class="card panel" id="windowForm"><div class="ct">Settings</div><label class="perm-row" style="border:0"><input type="checkbox" class="swc" id="w_enforce" ' + (w.enforce ? 'checked' : '') + '><span class="grow">Only take orders inside the window</span></label>' +
      '<div class="fields"><div class="field"><label for="w_open">Opens</label><input class="timebox" type="time" id="w_open" value="' + w.open + '" ' + (w.enforce ? '' : 'disabled') + '></div><div class="field"><label for="w_close">Closes</label><input class="timebox" type="time" id="w_close" value="' + w.close + '" ' + (w.enforce ? '' : 'disabled') + '></div></div>' +
      '<button class="btn btn-p btn-lg" type="submit">Save order window</button></form></div>';
  }

  function pAudit() {
    var q = ui.auditQ.trim().toLowerCase(), per = 12, types = state.audit.map(function (a) { return a.action; }).filter(function (x, i, arr) { return arr.indexOf(x) === i; }).sort();
    var list = state.audit.slice().sort(function (a, b) { return a.time < b.time ? 1 : -1; }).filter(function (a) { return (ui.auditType === 'all' || a.action === ui.auditType) && (!q || (a.user + ' ' + a.action + ' ' + a.detail).toLowerCase().indexOf(q) >= 0); });
    ui.auditPage = Math.min(ui.auditPage, Math.max(1, Math.ceil(list.length / per)));
    var page = list.slice((ui.auditPage - 1) * per, ui.auditPage * per);
    return head('Audit log', 'Who did what and when. Entries cannot be edited.', '<button class="btn" data-act="export-audit">' + icon('download', 18) + 'Export Excel</button>') +
      '<div class="row wrap" style="gap:10px"><input type="search" id="auditQ" class="search" placeholder="Search person or detail" value="' + esc(ui.auditQ) + '" aria-label="Search audit log"><select id="auditType" aria-label="Action" style="width:auto"><option value="all">All actions</option>' + types.map(function (t) { return '<option' + (ui.auditType === t ? ' selected' : '') + '>' + esc(t) + '</option>'; }).join('') + '</select><span class="muted mini">' + list.length + ' entries</span></div>' +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>When</th><th>Who</th><th>Action</th><th>Detail</th></tr></thead><tbody>' + (page.map(function (a) { return '<tr><td>' + dtt(a.time) + '</td><td>' + esc(a.user) + '</td><td><span class="pill neutral">' + esc(a.action) + '</span></td><td class="wrap">' + esc(a.detail) + '</td></tr>'; }).join('') || '<tr><td colspan="4" class="empty">No entries.</td></tr>') + '</tbody></table></div>' + pager(ui.auditPage, list.length, per, 'audit-page') + '</div>';
  }

  function field(id, label, input) { return '<div class="field"><label for="' + id + '">' + label + '</label>' + input + '</div>'; }
  function modalHtml() {
    var m = ui.modal; if (!m) return '';
    var title, body, edit;
    if (A.modals[m.kind]) { var def = A.modals[m.kind](m); title = def.title; body = def.body; edit = null; } else if (m.kind === 'product') {
      edit = m.id ? prod(m.id) : null; title = edit ? 'Edit product' : 'Add product';
      body = field('f_name', 'Product name', '<input id="f_name" name="name" required value="' + esc(edit ? edit.name : '') + '">') +
        '<div class="fields">' + field('f_cat', 'Category', '<select id="f_cat" name="cat">' + state.categories.map(function (c) { return '<option value="' + c.id + '"' + (edit && edit.cat === c.id ? ' selected' : '') + '>' + c.name + '</option>'; }).join('') + '</select>') +
        field('f_price', 'Price (৳)', '<input id="f_price" name="price" type="number" min="1" required value="' + (edit ? edit.price : '') + '">') + '</div>' +
        field('f_photo', 'Photo (optional)', '<input id="f_photo" type="file" accept="image/*">') + '<div class="muted mini" id="photoNote">' + (edit && (edit.imgData || edit.img) ? 'Current photo is kept unless you pick a new one.' : 'Without a photo, initials are shown.') + '</div>';
    } else if (m.kind === 'customer') {
      edit = m.id ? cust(m.id) : null; title = edit ? 'Edit customer' : 'Add customer';
      body = field('f_name', 'Shop name', '<input id="f_name" name="name" required value="' + esc(edit ? edit.name : '') + '">') +
        '<div class="fields">' + field('f_owner', 'Owner', '<input id="f_owner" name="owner" required value="' + esc(edit ? edit.owner : '') + '">') + field('f_phone', 'Phone', '<input id="f_phone" name="phone" inputmode="tel" value="' + esc(edit && edit.phone !== '—' ? edit.phone : '') + '">') + '</div>' +
        '<div class="fields">' + field('f_area', 'Area', '<input id="f_area" name="area" value="' + esc(edit && edit.area !== '—' ? edit.area : '') + '">') + field('f_limit', 'Credit limit (৳)', '<input id="f_limit" name="limit" type="number" min="0" value="' + (edit ? edit.limit : 50000) + '">') + '</div>';
    } else {
      edit = m.id ? state.users.filter(function (u) { return u.id === m.id; })[0] : null; title = edit ? 'Edit user' : 'Add user';
      body = field('f_name', 'Full name', '<input id="f_name" name="name" required value="' + esc(edit ? edit.name : '') + '">') +
        '<div class="fields">' + field('f_user', 'Username', '<input id="f_user" name="username" required autocomplete="off" value="' + esc(edit ? edit.username : '') + '">') +
        field('f_role', 'Role', '<select id="f_role" name="role">' + state.roles.map(function (r) { return '<option' + (edit && edit.role === r.name ? ' selected' : '') + '>' + esc(r.name) + '</option>'; }).join('') + '</select>') + '</div>' +
        (edit ? '' : field('f_pass', 'Temporary password', '<input id="f_pass" name="pass" type="password" required minlength="6" autocomplete="new-password">'));
    }
    return '<div class="modal-scrim" data-act="modal-close"></div><div class="modal card" role="dialog" aria-modal="true" aria-labelledby="mt"><div class="row between"><h2 class="ct" id="mt" style="font-size:20px">' + title + '</h2><button class="step s" data-act="modal-close" aria-label="Close">&times;</button></div>' +
      '<form id="modalForm" class="col" data-kind="' + m.kind + '" data-id="' + (m.id || '') + '" style="margin-top:14px">' + body + '<div class="row end" style="margin-top:6px"><button type="button" class="btn" data-act="modal-close">Cancel</button><button class="btn btn-p" type="submit">Save</button></div></form></div>';
  }
  function shrink(file, cb) {
    var fr = new FileReader();
    fr.onload = function () {
      var im = new Image();
      im.onload = function () { var s = 160 / Math.max(im.width, im.height, 160), c = document.createElement('canvas'); c.width = Math.round(im.width * s); c.height = Math.round(im.height * s); c.getContext('2d').drawImage(im, 0, 0, c.width, c.height); cb(c.toDataURL('image/jpeg', 0.8)); };
      im.src = fr.result;
    };
    fr.readAsDataURL(file);
  }

  // ---------- login ----------
  function loginPage(err) {
    return '<div class="auth"><div class="auth-hero"><div><img src="img/logo-yellow.png" alt="Talukder Foods"></div></div><main class="auth-form"><form id="loginForm"><div><h1 class="page-title" style="font-size:34px">Welcome back</h1><div class="page-sub">Sign in to take orders, invoice and collect.</div></div>' +
      '<div class="auth-hint"><b>Live demo.</b> Sample data, no server. Press Sign in as the administrator, or try <b>sabbir</b> (salesperson), <b>farhana</b> (accountant) or <b>tahmina</b> (sales manager). Password for all: <b>demo1234</b>.</div>' +
      (err ? '<div class="note bad" role="alert">' + esc(err) + '</div>' : '') +
      '<div class="field"><label for="u">Username</label><input id="u" value="' + esc(ui.loginU || 'admin') + '" autocomplete="username"></div><div class="field"><label for="p">Password</label><input id="p" type="password" value="' + (err ? '' : 'demo1234') + '" autocomplete="current-password"></div>' +
      '<button class="btn btn-p btn-lg btn-block" type="submit">Sign in</button></form></main></div>';
  }

  // ---------- router ----------
  function addRoute(path, spec) {
    var keys = [], re = new RegExp('^' + path.replace(/:(\w+)/g, function (_, k) { keys.push(k); return '(\\d+)'; }) + '$');
    spec.re = re; spec.keys = keys; A.routes.push(spec);
  }
  function route() {
    var h = location.hash.replace(/^#/, '') || '/notices';
    for (var i = 0; i < A.routes.length; i++) { var m = h.match(A.routes[i].re); if (m) return { spec: A.routes[i], args: m.slice(1).map(Number) }; }
    return { spec: A.routes.filter(function (r) { return r.nav === '#/notices'; })[0], args: [] };
  }
  function deniedPage() {
    return head('Access denied', 'Your role does not include this page.') + '<div class="card" style="max-width:560px"><div class="row" style="gap:16px"><div class="tile cat-1">' + icon('lock', 24) + '</div><div class="grow"><b>You need extra permission</b><div class="muted">Ask an administrator to change your role, or sign in as a different user. Roles are set under Admin, Roles.</div></div></div><div class="row" style="margin-top:16px"><a class="btn btn-p" href="#/notices">Go to notices</a>' + (can('dashboard') ? '<a class="btn" href="#/dashboard">Dashboard</a>' : '') + '</div></div>';
  }
  function render(keepScroll) {
    var y = window.scrollY, app = $('#app');
    if (!me()) { app.innerHTML = loginPage(ui.loginErr); document.title = 'Sign in · Talukder Foods Sales demo'; return; }
    var r = route(), sp = r.spec, ok = !sp.perm || can(sp.perm);
    var body = ok ? sp.html.apply(null, r.args) : deniedPage();
    app.innerHTML = layout({ nav: sp.nav }, body) + modalHtml(); document.title = (ok ? sp.title : 'Access denied') + ' · Talukder Foods Sales demo';
    window.scrollTo(0, keepScroll ? y : 0);
    var first = ui.modal && $('#modalForm input, #modalForm select'); if (first) first.focus({ preventScroll: true });
  }

  // ---------- actions ----------
  function nextNo(prefix, key) { state.seq[key]++; return prefix + (key === 'inv' ? 1000 + state.seq[key] : key === 'ret' ? 100 + state.seq[key] : 200 + state.seq[key]); }
  function makeInvoice(o) {
    var id = Math.max.apply(null, state.invoices.map(function (x) { return x.id; })) + 1, no = nextNo('INV-', 'inv');
    state.invoices.push({ id: id, no: no, date: today(), custId: o.custId, staff: o.staff, lines: o.lines, discount: 0 });
    o.status = 'invoiced'; o.invId = id;
    log('Invoice created', no + ' for ' + cust(o.custId).name + ' (' + money(linesTotal(o.lines)) + ')');
    return id;
  }
  function staffName() { var f = me().name.split(' ')[0]; return D.STAFF.indexOf(f) >= 0 ? f : 'Sabbir'; }
  function closeModal() { ui.modal = null; ui.modalImg = null; }

  document.addEventListener('click', function (e) {
    var go = e.target.closest('[data-go]'); if (go && !e.target.closest('a,button')) { location.hash = go.dataset.go; return; }
    var t = e.target.closest('[data-act]'); if (!t) return;
    var a = t.dataset.act, id = +t.dataset.id;
    if (A.actions[a]) { A.actions[a](t, id, e); return; }
    switch (a) {
      case 'logout': sessionStorage.removeItem(SESSION); closeModal(); location.hash = ''; render(); break;
      case 'reset': if (confirm('Reset the demo to its original sample data?')) { resetState(); ui.cart = { custId: null, lines: {}, override: false }; ui.picked = {}; closeModal(); toast('Demo data reset'); render(true); } break;
      case 'period': ui.period = t.dataset.v; render(true); break;
      case 'pick-cust': ui.cart.custId = id; ui.cart.override = false; render(true); break;
      case 'change-cust': ui.cart.custId = null; ui.custQ = ''; render(true); break;
      case 'prodcat': ui.prodCat = t.dataset.v; render(true); break;
      case 'step': var cur = ui.cart.lines[id] || 0; ui.cart.lines[id] = Math.max(0, cur + (+t.dataset.d)); render(true); break;
      case 'rm': delete ui.cart.lines[id]; render(true); break;
      case 'override': ui.cart.override = t.checked; render(true); break;
      case 'place':
        var c = ui.cart, ls = Object.keys(c.lines).filter(function (k) { return c.lines[k] > 0; }).map(function (k) { return { pid: +k, qty: c.lines[k], price: prod(+k).price }; });
        if (!c.custId || !ls.length) return;
        if (!windowStatus().open) { toast('The order desk is closed right now', true); return; }
        var cu = cust(c.custId), tot = linesTotal(ls);
        if (custDue(cu.id) + tot > cu.limit && !c.override) { toast('This order goes over ' + cu.name + "'s credit limit. Tick \"Allow anyway\" to continue.", true); return; }
        var oid = state.orders.length ? Math.max.apply(null, state.orders.map(function (x) { return x.id; })) + 1 : 1, no = nextNo('ORD-', 'ord');
        state.orders.push({ id: oid, no: no, date: today(), custId: c.custId, staff: staffName(), status: 'waiting', lines: ls });
        log('Order placed', no + ' for ' + cu.name + ' (' + money(tot) + ')'); notify('New order from ' + cu.name, no + ' · ' + money(tot) + '. It is waiting for an invoice.');
        save(); toast('Order placed for ' + cu.name); ui.cart = { custId: null, lines: {}, override: false }; location.hash = can('order.view') ? '#/orders' : '#/orders/new'; render(); break;
      case 'pick-order': ui.picked[id] = t.checked; render(true); break;
      case 'pick-all': waiting().forEach(function (o) { ui.picked[o.id] = t.checked; }); render(true); break;
      case 'invoice-one': var o1 = state.orders.filter(function (x) { return x.id === id; })[0]; var nid = makeInvoice(o1); save(); toast('Invoice created'); location.hash = '#/invoices/' + nid; break;
      case 'invoice-selected':
        var sel = waiting().filter(function (o) { return ui.picked[o.id]; }); if (!sel.length) return;
        sel.forEach(makeInvoice); ui.picked = {}; save(); toast(sel.length + ' invoice' + (sel.length > 1 ? 's' : '') + ' created'); location.hash = '#/invoices'; break;
      case 'inv-status': ui.invStatus = t.dataset.v; ui.invPage = 1; render(true); break;
      case 'inv-page': ui.invPage = +t.dataset.p; render(true); break;
      case 'col-page': ui.colPage = +t.dataset.p; render(true); break;
      case 'method': document.querySelectorAll('#method button').forEach(function (b) { b.classList.remove('on'); }); t.classList.add('on'); break;
      case 'collect':
        var inv = state.invoices.filter(function (i) { return i.id === id; })[0], amt = Math.round(+$('#amt').value), due = invDue(inv);
        if (!(amt > 0) || amt > due) { toast('Enter an amount between ৳ 1 and ' + money(due), true); return; }
        var meth = ($('#method .on') || {}).dataset ? $('#method .on').dataset.v : 'Cash';
        state.collections.push({ id: ++state.seq.col, date: today(), invId: inv.id, custId: inv.custId, amount: amt, method: meth });
        log('Payment recorded', money(amt) + ' (' + meth + ') against ' + inv.no); notify('Payment received', money(amt) + ' from ' + cust(inv.custId).name + ' against ' + inv.no);
        save(); toast(money(amt) + ' recorded for ' + inv.no); render(true); break;
      case 'print': window.print(); break;
      case 'modal': ui.modal = { kind: t.dataset.v, id: id || null, data: t.dataset }; ui.modalImg = null; render(true); break;
      case 'plan-day': ui.planDate = shiftDay(ui.planDate || today(), +t.dataset.d); render(true); break;
      case 'modal-close': closeModal(); render(true); break;
      case 'prod-toggle': var pr = prod(id); pr.active = pr.active === false; log(pr.active ? 'Product shown' : 'Product hidden', pr.name); save(); toast(pr.name + (pr.active ? ' is visible again' : ' hidden from new orders')); render(true); break;
      case 'user-toggle': var us = state.users.filter(function (u) { return u.id === id; })[0]; us.active = !us.active; log(us.active ? 'User enabled' : 'User disabled', us.username); save(); toast(us.name + (us.active ? ' enabled' : ' disabled')); render(true); break;
      case 'order-for': ui.cart = { custId: id, lines: {}, override: false }; location.hash = '#/orders/new'; break;
    }
  });

  document.addEventListener('change', function (e) {
    if (e.target.id === 'planDate' && e.target.value) { ui.planDate = e.target.value; render(true); }
    if (e.target.id === 'f_photo' && e.target.files[0]) shrink(e.target.files[0], function (d) { ui.modalImg = d; var n = $('#photoNote'); if (n) n.textContent = 'Photo ready.'; });
    A.onChange.forEach(function (fn) { fn(e); });
  });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && ui.modal) { closeModal(); render(true); } });

  function refocus(id) { var pos = document.activeElement && document.activeElement.selectionStart; render(true); var n = $('#' + id); if (n) { n.focus(); try { n.setSelectionRange(pos, pos); } catch (x) { /* ignore */ } } }
  A.refocus = refocus;

  document.addEventListener('input', function (e) {
    if (e.target.id === 'custQ') { ui.custQ = e.target.value; $('#custResults').innerHTML = custResults(); }
    if (e.target.id === 'invQ') { ui.invQ = e.target.value; ui.invPage = 1; refocus('invQ'); }
    A.onInput.forEach(function (fn) { fn(e); });
  });

  document.addEventListener('submit', function (e) {
    if (e.target.id === 'modalForm') {
      e.preventDefault();
      var f = e.target, k = f.dataset.kind, eid = +f.dataset.id || null, v = function (n) { return f.elements[n] ? f.elements[n].value.trim() : ''; };
      if (A.forms[k]) { if (A.forms[k](f, eid, v) === false) return; }
      else if (k === 'product') {
        var price = Math.round(+v('price')); if (!v('name') || !(price > 0)) { toast('Enter a name and a price above 0', true); return; }
        var pr = eid ? prod(eid) : { id: Math.max.apply(null, state.products.map(function (x) { return x.id; })) + 1, active: true };
        pr.name = v('name'); pr.cat = v('cat'); pr.price = price; if (ui.modalImg) { pr.imgData = ui.modalImg; }
        if (!eid) state.products.push(pr);
        log(eid ? 'Product updated' : 'Product added', pr.name + ' (' + money(pr.price) + ')'); toast(eid ? 'Product updated' : 'Product added');
      } else if (k === 'customer') {
        if (!v('name') || !v('owner')) { toast('Enter the shop and owner name', true); return; }
        var cs = eid ? cust(eid) : { id: Math.max.apply(null, state.customers.map(function (x) { return x.id; })) + 1 };
        cs.name = v('name'); cs.owner = v('owner'); cs.area = v('area') || '—'; cs.phone = v('phone') || '—'; cs.limit = Math.max(0, +v('limit') || 0);
        if (!eid) state.customers.push(cs);
        log(eid ? 'Customer updated' : 'Customer added', cs.name); toast(eid ? 'Customer updated' : 'Customer added');
      } else {
        var un = v('username').toLowerCase();
        if (!v('name') || !un) { toast('Enter a name and username', true); return; }
        if (state.users.some(function (x) { return x.username === un && x.id !== eid; })) { toast('That username is taken', true); return; }
        if (eid) { var uu = state.users.filter(function (x) { return x.id === eid; })[0]; uu.name = v('name'); uu.username = un; uu.role = v('role'); }
        else state.users.push({ id: Math.max.apply(null, state.users.map(function (x) { return x.id; })) + 1, name: v('name'), username: un, role: v('role'), active: true, pass: v('pass') });
        log(eid ? 'User updated' : 'User added', un + ' (' + v('role') + ')'); toast(eid ? 'User updated' : 'User added');
      }
      save(); closeModal(); render(true); return;
    }
    if (e.target.id === 'loginForm') {
      e.preventDefault();
      var un2 = $('#u').value.trim().toLowerCase(), pw = $('#p').value, u = state.users.filter(function (x) { return x.username === un2; })[0];
      ui.loginU = un2;
      if (!u || u.pass !== pw) ui.loginErr = 'Wrong username or password.';
      else if (!u.active) ui.loginErr = 'This account is disabled.';
      else { ui.loginErr = ''; sessionStorage.setItem(SESSION, u.id); location.hash = can('dashboard') ? '#/dashboard' : '#/notices'; }
      render();
    }
  });

  // chart hover
  document.addEventListener('mousemove', function (e) {
    var w = e.target.closest && e.target.closest('.chart-wrap'); if (!w) return;
    var rows = JSON.parse(decodeURIComponent(w.dataset.chart)), L = +w.dataset.l, W = +w.dataset.w, R = +w.dataset.r;
    var box = w.getBoundingClientRect(), px = (e.clientX - box.left) / box.width * W;
    var i = Math.max(0, Math.min(rows.length - 1, Math.round((px - L) / ((W - L - R) / Math.max(1, rows.length - 1)))));
    var cx = L + (rows.length === 1 ? (W - L - R) / 2 : i * (W - L - R) / (rows.length - 1)), tip = $('.tip', w), xh = $('.xh', w);
    xh.setAttribute('x1', cx); xh.setAttribute('x2', cx); xh.setAttribute('opacity', '1');
    tip.innerHTML = '<b>' + rows[i][0] + '</b><div class="r2"><span><i class="key-line" style="background:#E5412D"></i> Billed</span><b class="num">' + money(rows[i][1]) + '</b></div><div class="r2"><span><i class="key-line" style="background:#1F9E96"></i> Collected</span><b class="num">' + money(rows[i][2]) + '</b></div>';
    var left = cx / W * box.width; left = Math.max(95, Math.min(box.width - 95, left));
    tip.style.left = left + 'px'; tip.style.opacity = '1';
  });
  document.addEventListener('mouseout', function (e) {
    var w = e.target.closest && e.target.closest('.chart-wrap'); if (!w || w.contains(e.relatedTarget)) return;
    $('.tip', w).style.opacity = '0'; $('.xh', w).setAttribute('opacity', '0');
  });

  window.addEventListener('hashchange', function () { render(); });

  // ---------- handlers for the extra pages ----------
  function planItems(day) {
    var src = state.invoices.filter(function (i) { return i.date === day; }).concat(state.orders.filter(function (o) { return o.status === 'waiting' && o.date === day; })), qty = {};
    src.forEach(function (o) { o.lines.forEach(function (l) { qty[l.pid] = (qty[l.pid] || 0) + l.qty; }); });
    return state.products.filter(function (p) { return qty[p.id]; }).map(function (p) { return [cat(p.cat).name, p.name, qty[p.id]]; });
  }
  var EXPORTS = {
    'export-invoices': function () { csv('invoices.csv', [['Invoice', 'Date', 'Customer', 'Salesperson', 'Total', 'Paid', 'Due', 'Status']].concat(filteredInvoices().filter(function (i) { return ui.invStatus === 'all' || invStatus(i) === ui.invStatus; }).map(function (i) { return [i.no, i.date, cust(i.custId).name, i.staff, invTotal(i), invPaid(i), invDue(i), invStatus(i)]; }))); },
    'export-collections': function () { csv('collections.csv', [['Date', 'Customer', 'Invoice', 'Method', 'Amount']].concat(state.collections.slice().sort(function (a, b) { return b.id - a.id; }).map(function (c) { var i = state.invoices.filter(function (x) { return x.id === c.invId; })[0]; return [c.date, cust(c.custId).name, i ? i.no : '', c.method, c.amount]; }))); },
    'export-products': function () { csv('products.csv', [['Product', 'Category', 'Price', 'Visible']].concat(state.products.map(function (p) { return [p.name, cat(p.cat).name, p.price, p.active === false ? 'No' : 'Yes']; }))); },
    'export-customers': function () { csv('customers.csv', [['Shop', 'Owner', 'Area', 'Phone', 'Credit limit', 'Billed (30d)', 'Owes']].concat(state.customers.map(function (c) { return [c.name, c.owner, c.area, c.phone, c.limit, custBilled(c.id), custDue(c.id)]; }))); },
    'export-audit': function () { csv('audit-log.csv', [['When', 'Who', 'Action', 'Detail']].concat(state.audit.slice().sort(function (a, b) { return a.time < b.time ? 1 : -1; }).map(function (a) { return [a.time.replace('T', ' '), a.user, a.action, a.detail]; }))); },
    'export-production': function () { var d = ui.planDate || today(); csv('production-plan-' + d + '.csv', [['Category', 'Product', 'Packs']].concat(planItems(d))); }
  };
  Object.keys(EXPORTS).forEach(function (k) { A.actions[k] = EXPORTS[k]; });
  A.actions['export-statement'] = function (t, id) { var st = statementRows(id); csv('statement-' + st.c.name.replace(/\W+/g, '-').toLowerCase() + '.csv', [['Date', 'Reference', 'Details', 'Billed', 'Paid or credited', 'Balance']].concat(st.rows.map(function (e) { return [e.date, e.ref, e.what, e.debit || '', e.credit || '', e.bal]; })).concat([['', '', 'Closing balance', '', '', st.closing]])); };

  function done(msg) { save(); if (msg) toast(msg); render(true); }
  A.actions['order-cancel'] = function (t, id) {
    var o = state.orders.filter(function (x) { return x.id === id; })[0];
    if (!confirm('Cancel order ' + o.no + ' for ' + cust(o.custId).name + '?')) return;
    o.status = 'cancelled'; delete ui.picked[id]; log('Order cancelled', o.no + ' for ' + cust(o.custId).name); done('Order ' + o.no + ' cancelled');
  };
  A.actions['note-read'] = function (t, id) { state.notifications.forEach(function (n) { if (n.id === id) n.read = true; }); done(); };
  A.actions['note-all'] = function () { state.notifications.forEach(function (n) { n.read = true; }); done('All notifications marked as read'); };
  A.actions['note-filter'] = function (t) { ui.noteFilter = t.dataset.v; render(true); };
  A.actions['notice-delete'] = function (t, id) { var n = state.notices.filter(function (x) { return x.id === id; })[0]; if (!confirm('Remove the notice "' + n.title + '"?')) return; state.notices = state.notices.filter(function (x) { return x.id !== id; }); log('Notice removed', n.title); done('Notice removed'); };
  A.actions['role-sel'] = function (t, id) { ui.roleSel = id; render(true); };
  A.actions['perm-toggle'] = function (t) {
    var r = state.roles.filter(function (x) { return x.id === (ui.roleSel || state.roles[0].id); })[0], k = t.dataset.k, i = r.perms.indexOf(k);
    if (t.checked && i < 0) r.perms.push(k); if (!t.checked && i >= 0) r.perms.splice(i, 1);
    log('Role changed', r.name + ': ' + k + (t.checked ? ' on' : ' off')); save(); toast(r.name + ' updated'); render(true);
  };
  A.actions['role-delete'] = function (t, id) { var r = state.roles.filter(function (x) { return x.id === id; })[0]; if (!confirm('Delete the role "' + r.name + '"?')) return; state.roles = state.roles.filter(function (x) { return x.id !== id; }); ui.roleSel = null; log('Role deleted', r.name); done('Role deleted'); };
  A.actions['cat-delete'] = function (t) { var c = cat(t.dataset.cid); state.categories = state.categories.filter(function (x) { return x.id !== t.dataset.cid; }); log('Category deleted', c.name); done('Category deleted'); };
  A.actions['cust-delete'] = function (t, id) { var c = cust(id); if (!confirm('Delete ' + c.name + '? This customer has no invoices.')) return; state.customers = state.customers.filter(function (x) { return x.id !== id; }); log('Customer deleted', c.name); save(); toast('Customer deleted'); location.hash = '#/customers'; render(); };
  A.actions['cust-page'] = function (t) { ui.custPage = +t.dataset.p; render(true); };
  A.actions['audit-page'] = function (t) { ui.auditPage = +t.dataset.p; render(true); };
  A.actions['st-clear'] = function () { ui.stFrom = ''; ui.stTo = ''; render(true); };
  A.actions['signin-as'] = function (t, id) {
    var u = state.users.filter(function (x) { return x.id === id; })[0]; log('Signed in as', u.name + ' (' + u.role + ')'); sessionStorage.setItem(SESSION, u.id); save();
    toast('Now signed in as ' + u.name); location.hash = can('dashboard') ? '#/dashboard' : '#/notices'; render();
  };
  A.actions['return-submit'] = function (t, id) {
    var inv = state.invoices.filter(function (x) { return x.id === id; })[0], ls = [];
    document.querySelectorAll('.rq').forEach(function (n) { var q = Math.floor(+n.value || 0), max = +n.max; if (q > max) q = max; if (q > 0) ls.push({ pid: +n.dataset.pid, qty: q, price: +n.dataset.price }); });
    if (!ls.length) { toast('Enter a quantity to return', true); return; }
    var no = nextNo('CN-', 'ret'), amt = linesTotal(ls);
    state.returns.push({ id: state.seq.ret, no: no, invId: id, custId: inv.custId, date: today(), lines: ls, amount: amt });
    log('Return recorded', no + ' for ' + inv.no + ': ' + money(amt)); notify('Credit note ' + no, money(amt) + ' credited to ' + cust(inv.custId).name + ' on ' + inv.no);
    save(); toast('Credit note ' + no + ' created'); location.hash = '#/invoices/' + id; render();
  };

  // modals
  var fld = function (id, label, input) { return field(id, label, input); };
  A.modals['order-edit'] = function (m) {
    var o = state.orders.filter(function (x) { return x.id === m.id; })[0];
    return { title: 'Edit order ' + o.no, body: '<div class="muted">' + esc(cust(o.custId).name) + '. Set a quantity to 0 to remove an item.</div>' +
      o.lines.map(function (l) { return '<div class="row"><div class="grow"><b>' + esc(prod(l.pid).name) + '</b><div class="muted mini">' + money(l.price) + ' each</div></div><input type="number" min="0" name="q_' + l.pid + '" value="' + l.qty + '" aria-label="Quantity of ' + esc(prod(l.pid).name) + '" style="width:100px;text-align:right"></div>'; }).join('') +
      '<div class="fields">' + fld('add_pid', 'Add an item', '<select id="add_pid" name="add_pid"><option value="">None</option>' + state.products.filter(function (p) { return p.active !== false && !o.lines.some(function (l) { return l.pid === p.id; }); }).map(function (p) { return '<option value="' + p.id + '">' + esc(p.name) + '</option>'; }).join('') + '</select>') + fld('add_qty', 'Quantity', '<input id="add_qty" name="add_qty" type="number" min="1" value="10">') + '</div>' };
  };
  A.forms['order-edit'] = function (f, id) {
    var o = state.orders.filter(function (x) { return x.id === id; })[0], ls = [];
    o.lines.forEach(function (l) { var q = Math.floor(+f.elements['q_' + l.pid].value || 0); if (q > 0) ls.push({ pid: l.pid, qty: q, price: l.price }); });
    if (f.elements.add_pid.value) ls.push({ pid: +f.elements.add_pid.value, qty: Math.max(1, Math.floor(+f.elements.add_qty.value || 1)), price: prod(+f.elements.add_pid.value).price });
    if (!ls.length) { toast('An order needs at least one item. Use Cancel to remove it.', true); return false; }
    o.lines = ls; log('Order edited', o.no + ' now ' + money(linesTotal(ls))); toast('Order ' + o.no + ' updated');
  };
  A.modals['discount'] = function (m) { var inv = state.invoices.filter(function (x) { return x.id === m.id; })[0]; return { title: 'Give a discount on ' + inv.no, body: '<div class="muted">Due on this invoice: <b class="num">' + money(invDue(inv)) + '</b>. The discount lowers the total.</div>' + fld('d_amt', 'Discount amount (৳)', '<input id="d_amt" name="amt" type="number" min="1" max="' + invDue(inv) + '" required>') + fld('d_why', 'Reason', '<input id="d_why" name="why" placeholder="e.g. damaged pack, loyal customer">') }; };
  A.forms['discount'] = function (f, id, v) {
    var inv = state.invoices.filter(function (x) { return x.id === id; })[0], amt = Math.round(+v('amt'));
    if (!(amt > 0) || amt > invDue(inv)) { toast('Enter an amount between ৳ 1 and ' + money(invDue(inv)), true); return false; }
    inv.discount = (inv.discount || 0) + amt; log('Discount given', money(amt) + ' on ' + inv.no + (v('why') ? ' (' + v('why') + ')' : '')); toast('Discount of ' + money(amt) + ' applied');
  };
  A.modals['collect-cust'] = function (m) {
    var owing = state.customers.filter(function (c) { return custDue(c.id) > 0; });
    var who = m.id ? '<div class="selected-cust"><div class="avatar big">' + initials(cust(m.id).name) + '</div><div class="grow"><b>' + esc(cust(m.id).name) + '</b></div><div><div class="lbl">Owes now</div><b class="num">' + money(custDue(m.id)) + '</b></div></div><input type="hidden" name="cid" value="' + m.id + '">' :
      fld('c_cust', 'Customer', '<select id="c_cust" name="cid">' + owing.map(function (c) { return '<option value="' + c.id + '">' + esc(c.name) + ' (owes ' + money(custDue(c.id)) + ')</option>'; }).join('') + '</select>');
    return { title: 'Collect from customer', body: '<div class="muted">One payment. It clears the oldest unpaid invoices first.</div>' + who + '<div class="fields">' + fld('c_amt', 'Amount received (৳)', '<input id="c_amt" name="amt" type="number" min="1" required' + (m.id ? ' value="' + custDue(m.id) + '"' : '') + '>') + fld('c_meth', 'Method', '<select id="c_meth" name="meth"><option>Cash</option><option>bKash</option><option>Bank</option></select>') + '</div>' };
  };
  A.forms['collect-cust'] = function (f, id, v) {
    var cid = +v('cid'), amt = Math.round(+v('amt')), due = custDue(cid);
    if (!cid || !(amt > 0) || amt > due) { toast('Enter an amount between ৳ 1 and ' + money(due), true); return false; }
    var left = amt, n = 0;
    state.invoices.filter(function (i) { return i.custId === cid && invDue(i) > 0; }).sort(function (a, b) { return a.id - b.id; }).forEach(function (i) {
      if (left <= 0) return; var pay = Math.min(left, invDue(i)); state.collections.push({ id: ++state.seq.col, date: today(), invId: i.id, custId: cid, amount: pay, method: v('meth') }); left -= pay; n++;
    });
    log('Payment recorded', money(amt) + ' (' + v('meth') + ') from ' + cust(cid).name + ', cleared ' + n + ' invoice' + (n === 1 ? '' : 's')); notify('Payment received', money(amt) + ' from ' + cust(cid).name + ' applied to ' + n + ' invoice' + (n === 1 ? '' : 's')); toast(money(amt) + ' applied to ' + n + ' invoice' + (n === 1 ? '' : 's'));
  };
  A.modals['password'] = function (m) { var u = state.users.filter(function (x) { return x.id === m.id; })[0]; return { title: (me().id === u.id ? 'Change password' : 'Reset password') + ' for ' + u.name, body: fld('pw1', 'New password', '<input id="pw1" name="pw" type="password" minlength="6" required autocomplete="new-password">') + '<div class="muted mini">At least 6 characters.</div>' }; };
  A.forms['password'] = function (f, id, v) { var u = state.users.filter(function (x) { return x.id === id; })[0]; if (v('pw').length < 6) { toast('Use at least 6 characters', true); return false; } u.pass = v('pw'); log('Password changed', u.username); toast('Password updated for ' + u.name); };
  A.modals['notice'] = function () { return { title: 'Post a notice', body: fld('n_t', 'Title', '<input id="n_t" name="title" required>') + fld('n_b', 'Message', '<textarea id="n_b" name="body" rows="4" required></textarea>') }; };
  A.forms['notice'] = function (f, id, v) { if (!v('title') || !v('body')) { toast('Add a title and a message', true); return false; } state.notices.push({ id: ++state.seq.notice, date: today(), title: v('title'), body: v('body'), by: me().name }); log('Notice posted', v('title')); notify('New notice', v('title')); toast('Notice posted'); };
  A.modals['role'] = function () { return { title: 'Add role', body: fld('r_n', 'Role name', '<input id="r_n" name="name" required>') + fld('r_c', 'Start from', '<select id="r_c" name="copy"><option value="">No permissions</option>' + state.roles.map(function (r) { return '<option>' + esc(r.name) + '</option>'; }).join('') + '</select>') }; };
  A.forms['role'] = function (f, id, v) {
    if (!v('name')) { toast('Give the role a name', true); return false; } if (state.roles.some(function (r) { return r.name.toLowerCase() === v('name').toLowerCase(); })) { toast('That role already exists', true); return false; }
    var src = roleOf(v('copy')), r = { id: ++state.seq.role, name: v('name'), perms: src ? src.perms.slice() : [] }; state.roles.push(r); ui.roleSel = r.id; log('Role added', r.name); toast('Role added');
  };
  A.modals['category'] = function (m) { var c = m.id ? state.categories[m.id - 1] : null; return { title: c ? 'Rename category' : 'Add category', body: fld('k_n', 'Category name', '<input id="k_n" name="name" required value="' + esc(c ? c.name : '') + '">') }; };
  A.forms['category'] = function (f, id, v) {
    if (!v('name')) { toast('Give the category a name', true); return false; }
    if (id) { var c = state.categories[id - 1]; log('Category renamed', c.name + ' to ' + v('name')); c.name = v('name'); toast('Category renamed'); }
    else { var n = state.categories.length; state.categories.push({ id: 'c' + Date.now(), name: v('name'), cls: CAT_CLS[n % CAT_CLS.length], color: CAT_COL[n % CAT_COL.length] }); log('Category added', v('name')); toast('Category added'); }
  };

  // live inputs and selects
  A.onChange.push(function (e) {
    var id = e.target.id, v = e.target.value;
    if (id === 'invDays') { ui.invDays = v; ui.invFrom = ''; ui.invTo = ''; ui.invPage = 1; render(true); }
    else if (id === 'invFrom' || id === 'invTo') { ui[id] = v; ui.invPage = 1; render(true); }
    else if (id === 'stFrom' || id === 'stTo') { ui[id] = v; render(true); }
    else if (id === 'auditType') { ui.auditType = v; ui.auditPage = 1; render(true); }
    else if (id === 'w_enforce') { ['w_open', 'w_close'].forEach(function (k) { $('#' + k).disabled = !e.target.checked; }); }
  });
  A.onInput.push(function (e) {
    var id = e.target.id, v = e.target.value;
    if (id === 'ordQ') { ui.ordQ = v; A.refocus(id); }
    else if (id === 'prodQ') { ui.prodQ = v; A.refocus(id); }
    else if (id === 'custFind') { ui.custFind = v; ui.custPage = 1; A.refocus(id); }
    else if (id === 'auditQ') { ui.auditQ = v; ui.auditPage = 1; A.refocus(id); }
    else if (e.target.classList && e.target.classList.contains('rq')) { var t = 0; document.querySelectorAll('.rq').forEach(function (n) { t += Math.min(+n.max, Math.max(0, Math.floor(+n.value || 0))) * +n.dataset.price; }); $('#creditTotal').textContent = money(t); }
  });
  document.addEventListener('submit', function (e) {
    if (e.target.id !== 'windowForm') return; e.preventDefault();
    var on = $('#w_enforce').checked, o = $('#w_open').value, c = $('#w_close').value;
    if (on && (!o || !c || o >= c)) { toast('Closing time must be after opening time', true); return; }
    state.window.enforce = on; if (on) { state.window.open = o; state.window.close = c; }
    log('Order window changed', on ? fmtTime(o) + ' to ' + fmtTime(c) : 'Not restricted'); notify('Order window changed', on ? 'Orders are open from ' + fmtTime(o) + ' to ' + fmtTime(c) : 'Orders are no longer restricted by time'); save(); toast('Order window saved'); render(true);
  });

  addRoute('/notices', { nav: '#/notices', title: 'Notices', html: function () { return pNotices(); } });
  addRoute('/notifications', { nav: '#/notifications', title: 'Notifications', html: function () { return pNotifications(); } });
  addRoute('/invoices/:id/return', { nav: '#/invoices', title: 'Return items', perm: 'invoice.return', html: function (id) { return pReturn(id); } });
  addRoute('/categories', { nav: '#/categories', title: 'Categories', perm: 'products', html: function () { return pCategories(); } });
  addRoute('/roles', { nav: '#/roles', title: 'Roles', perm: 'roles', html: function () { return pRoles(); } });
  addRoute('/window', { nav: '#/window', title: 'Order window', perm: 'window', html: function () { return pWindow(); } });
  addRoute('/audit', { nav: '#/audit', title: 'Audit log', perm: 'audit', html: function () { return pAudit(); } });

  // ---------- routes for the pages in this file ----------
  addRoute('/dashboard', { nav: '#/dashboard', title: 'Dashboard', perm: 'dashboard', html: function () { return pDashboard(); } });
  addRoute('/reports', { nav: '#/reports', title: 'KPI report', perm: 'reports', html: function () { return pReports(); } });
  addRoute('/orders/new', { nav: '#/orders/new', title: 'New order', perm: 'order.create', html: function () { return pNewOrder(); } });
  addRoute('/orders', { nav: '#/orders', title: 'Orders to invoice', perm: 'order.view', html: function () { return pOrders(); } });
  addRoute('/invoices', { nav: '#/invoices', title: 'Invoices', perm: 'invoice.view', html: function () { return pInvoices(); } });
  addRoute('/invoices/:id', { nav: '#/invoices', title: 'Invoice', perm: 'invoice.view', html: function (id) { return pInvoice(id); } });
  addRoute('/collections', { nav: '#/collections', title: 'Collections', perm: 'collection.view', html: function () { return pCollections(); } });
  addRoute('/products', { nav: '#/products', title: 'Products', perm: 'products', html: function () { return pProducts(); } });
  addRoute('/production', { nav: '#/production', title: 'Production plan', perm: 'production', html: function () { return pProduction(); } });
  addRoute('/users', { nav: '#/users', title: 'Users', perm: 'users', html: function () { return pUsers(); } });
  addRoute('/customers', { nav: '#/customers', title: 'Customers', perm: 'customers', html: function () { return pCustomers(); } });
  addRoute('/customers/:id', { nav: '#/customers', title: 'Customer', perm: 'customers', html: function (id) { return pCustomer(id); } });

  // ---------- shared with the other script files ----------
  A.addRoute = addRoute; A.render = render; A.save = save; A.closeModal = closeModal;
  A.h = { $: $, esc: esc, money: money, short: short, dt: dt, dtt: dtt, today: today, nowIso: nowIso, fmtTime: fmtTime, cust: cust, prod: prod, cat: cat, initials: initials, sum: sum, linesTotal: linesTotal, creditTotal: creditTotal, invTotal: invTotal, invPaid: invPaid, invDue: invDue, invStatus: invStatus, ageDays: ageDays, isoOf: isoOf, inRange: inRange, icon: icon, toast: toast, head: head, pager: pager, field: field, delta: delta, thumb: thumb, custDue: custDue, custBilled: custBilled, waiting: waiting, outstanding: outstanding, stats: stats, log: log, notify: notify, can: can, me: me, roleOf: roleOf, csv: csv, windowStatus: windowStatus, nextNo: nextNo, makeInvoice: makeInvoice, shiftDay: shiftDay, pill: null, donut: donut };
  A.start = function () { render(); };
})();
