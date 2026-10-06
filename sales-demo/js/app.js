(function () {
  'use strict';

  var KEY = 'talukder-sales-demo-v1', SESSION = 'talukder-sales-demo-user';
  var D = window.DEMO;
  var state = load();
  var ui = { period: '7', invStatus: 'all', invQ: '', invPage: 1, colPage: 1, cart: { custId: null, lines: {} }, prodCat: 'all', custQ: '', picked: {} };

  // ---------- storage ----------
  function load() {
    try { var s = JSON.parse(localStorage.getItem(KEY)); if (s && s.v === 1) return s; } catch (e) { /* fall through */ }
    var s2 = D.seed(); save(s2); return s2;
  }
  function save(s) { try { localStorage.setItem(KEY, JSON.stringify(s || state)); } catch (e) { /* ignore */ } }

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
  var cat = function (id) { return D.CATS.filter(function (c) { return c.id === id; })[0]; };
  var initials = function (n) { return n.split(/\s+/).slice(0, 2).map(function (w) { return w[0]; }).join('').toUpperCase(); };
  var linesTotal = function (ls) { return ls.reduce(function (s, l) { return s + l.qty * l.price; }, 0); };
  var invTotal = function (i) { return linesTotal(i.lines) - (i.discount || 0); };
  var invPaid = function (i) { return state.collections.filter(function (c) { return c.invId === i.id; }).reduce(function (s, c) { return s + c.amount; }, 0); };
  var invDue = function (i) { return Math.max(0, invTotal(i) - invPaid(i)); };
  var invStatus = function (i) { var p = invPaid(i), t = invTotal(i); return p >= t ? 'paid' : p > 0 ? 'partial' : 'unpaid'; };
  var ageDays = function (iso) { return Math.round((new Date(today() + 'T00:00:00') - new Date(iso + 'T00:00:00')) / 864e5); };
  var sum = function (a, f) { return a.reduce(function (s, x) { return s + f(x); }, 0); };
  var isoOf = function (offset) { return D.day(offset); };
  var inRange = function (iso, from, to) { return iso >= from && iso <= to; };

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
    ['Overview', [['Dashboard', '#/dashboard', 'dashboard'], ['KPI report', '#/reports', 'reports']]],
    ['Sell', [['New order', '#/orders/new', 'new-order'], ['Orders to invoice', '#/orders', 'orders'], ['Invoices', '#/invoices', 'invoices'], ['Collections', '#/collections', 'collections']]],
    ['Operate', [['Products', '#/products', 'products'], ['Customers', '#/customers', 'customers']]]
  ];

  function layout(route, body) {
    var wc = waiting().length;
    var nav = NAV.map(function (g) {
      return '<div class="nav-group"><div class="nav-label">' + g[0] + '</div>' + g[1].map(function (i) {
        var on = route.nav === i[1];
        var badge = i[1] === '#/orders' && wc ? '<span class="badge">' + wc + '</span>' : '';
        return '<a href="' + i[1] + '" class="' + (on ? 'on' : '') + '"' + (on ? ' aria-current="page"' : '') + '>' + icon(i[2]) + '<span class="grow">' + i[0] + '</span>' + badge + '</a>';
      }).join('') + '</div>';
    }).join('');
    return '<div class="shell"><aside class="side"><a class="logo-tile" href="#/dashboard" aria-label="Talukder Foods home"><img src="img/logo-purple.jpg" alt="Talukder Foods"></a>' +
      '<nav class="nav" aria-label="Main">' + nav + '</nav>' +
      '<div class="me"><div class="avatar">AD</div><div class="who"><b>Demo Admin</b><small>Administrator</small></div><button data-act="logout" aria-label="Sign out" title="Sign out">' + icon('logout', 18) + '</button></div></aside>' +
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
    var byCat = D.CATS.map(function (c) { return { name: c.name, color: c.color, v: sum(s.inv, function (i) { return sum(i.lines.filter(function (l) { return prod(l.pid).cat === c.id; }), function (l) { return l.qty * l.price; }); }) }; }).filter(function (c) { return c.v > 0; }).sort(function (a, b) { return b.v - a.v; });
    var credit = state.customers.filter(function (c) { return custDue(c.id) >= c.limit * 0.6; });
    var hour = new Date().getHours(), greet = hour < 12 ? 'Good morning' : hour < 17 ? 'Good afternoon' : 'Good evening';
    var periods = [['1', 'Today'], ['7', '7 days'], ['30', '30 days']];
    var attn = '';
    if (w.length) attn += '<a href="#/orders" class="attn bad"><span class="ico">' + icon('orders') + '</span><span class="grow"><b>' + w.length + ' orders waiting for an invoice</b><small>' + money(sum(w, function (o) { return linesTotal(o.lines); })) + '</small></span><span class="cta">Invoice &rarr;</span></a>';
    if (credit.length) attn += '<a href="#/customers" class="attn warn"><span class="ico">' + icon('customers') + '</span><span class="grow"><b>' + credit.length + ' customers near their credit limit</b><small>' + esc(credit.slice(0, 2).map(function (c) { return c.name; }).join(', ')) + (credit.length > 2 ? ' and ' + (credit.length - 2) + ' more' : '') + '</small></span><span class="cta">Review &rarr;</span></a>';
    if (over30.length) attn += '<a href="#/invoices" class="attn neutral"><span class="ico">' + icon('collections') + '</span><span class="grow"><b>' + over30.length + ' invoices over 30 days late</b><small>' + money(sum(over30, invDue)) + ' in total</small></span><span class="cta">Collect &rarr;</span></a>';
    attn += '<a href="#/orders/new" class="attn good"><span class="ico">' + icon('clock') + '</span><span class="grow"><b>Order window is open</b><small>Take orders until 6:00 PM</small></span><span class="cta">New order &rarr;</span></a>';
    return head(greet + ', Admin', new Date().toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long' }) + ' <span class="pill good" style="margin-left:8px">' + icon('clock', 14) + 'Order window open until 6:00 PM</span>',
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
    var c = ui.cart, selected = c.custId ? cust(c.custId) : null;
    var lines = Object.keys(c.lines).filter(function (k) { return c.lines[k] > 0; }).map(function (k) { var p = prod(+k); return { p: p, qty: c.lines[k] }; });
    var total = sum(lines, function (l) { return l.qty * l.p.price; }), units = sum(lines, function (l) { return l.qty; });
    var custBlock = selected ?
      '<div class="selected-cust"><div class="avatar big">' + initials(selected.name) + '</div><div class="grow"><b>' + esc(selected.name) + '</b><div class="muted">' + esc(selected.owner) + ' · ' + esc(selected.area) + '</div></div><div><div class="lbl">Owes now</div><b class="num">' + money(custDue(selected.id)) + '</b></div><button class="btn btn-s" data-act="change-cust">Change</button></div>' :
      '<div class="field"><label for="custQ">Find customer</label><input id="custQ" type="search" placeholder="Search by shop or owner name" value="' + esc(ui.custQ) + '" autocomplete="off"></div><div class="cust-results" id="custResults">' + custResults() + '</div>';
    var chips = '<button class="chip ' + (ui.prodCat === 'all' ? 'on' : '') + '" data-act="prodcat" data-v="all">All</button>' + D.CATS.filter(function (x) { return x.id !== 'cake'; }).concat(D.CATS.filter(function (x) { return x.id === 'cake'; })).map(function (x) { return '<button class="chip ' + (ui.prodCat === x.id ? 'on' : '') + '" data-act="prodcat" data-v="' + x.id + '">' + x.name + '</button>'; }).join('');
    var grid = state.products.filter(function (p) { return ui.prodCat === 'all' || p.cat === ui.prodCat; }).map(function (p) {
      var q = c.lines[p.id] || 0;
      return '<div class="product ' + (q ? 'has' : '') + '"><div class="row"><img class="thumb" src="img/' + p.img + '.jpg" alt=""><div class="grow"><b>' + esc(p.name) + '</b><div class="muted mini">' + cat(p.cat).name + '</div><div class="num strong">' + money(p.price) + '</div></div></div>' +
        '<div class="row stepper"><button class="step" data-act="step" data-id="' + p.id + '" data-d="-5" aria-label="Remove 5 ' + esc(p.name) + '">−</button><div class="qty num" aria-live="polite">' + q + '</div><button class="step" data-act="step" data-id="' + p.id + '" data-d="5" aria-label="Add 5 ' + esc(p.name) + '">+</button></div></div>';
    }).join('');
    var cartHtml = lines.length ? lines.map(function (l) { return '<div class="cart-line"><div class="grow"><b>' + esc(l.p.name) + '</b><div class="muted mini">' + l.qty + ' × ' + money(l.p.price) + '</div></div><b class="num">' + money(l.qty * l.p.price) + '</b><button class="rm" data-act="rm" data-id="' + l.p.id + '" aria-label="Remove ' + esc(l.p.name) + '">' + icon('trash', 16) + '</button></div>'; }).join('') : '<div class="empty-dash">Add products to start the order.</div>';
    var ready = selected && lines.length;
    return head('New order', 'Pick the customer, then tap + on the products they want. Items go up by 5.') +
      '<div class="flow top"><div class="col grow" style="gap:20px;flex:2 1 560px"><div class="card"><div class="row" style="margin-bottom:12px"><span class="step-no">1</span><div class="ct">Customer</div></div>' + custBlock + '</div>' +
      '<div class="card"><div class="row between wrap" style="margin-bottom:12px"><div class="row"><span class="step-no">2</span><div class="ct">Products</div></div><div class="chips">' + chips + '</div></div><div class="product-grid">' + grid + '</div></div></div>' +
      '<aside class="card cart"><div class="ct">Order summary</div><div>' + cartHtml + '</div><div class="row between"><span class="lbl">' + units + ' units</span><b class="num" style="font-size:22px">' + money(total) + '</b></div>' +
      '<button class="btn btn-p btn-lg btn-block ' + (ready ? '' : 'is-off') + '" data-act="place" ' + (ready ? '' : 'disabled') + '>Place order</button>' + (ready ? '' : '<div class="muted mini" style="text-align:center">' + (selected ? 'Add at least one product.' : 'Choose a customer first.') + '</div>') + '</aside></div>';
  }
  function custResults() {
    var q = ui.custQ.trim().toLowerCase();
    var list = state.customers.filter(function (c) { return !q || (c.name + ' ' + c.owner + ' ' + c.area).toLowerCase().indexOf(q) >= 0; }).slice(0, 6);
    return list.length ? list.map(function (c) { return '<button class="cust" data-act="pick-cust" data-id="' + c.id + '"><b>' + esc(c.name) + '</b><small>' + esc(c.owner) + ' · ' + esc(c.area) + '</small></button>'; }).join('') : '<div class="empty">No customer matches.</div>';
  }

  function pOrders() {
    var w = waiting();
    var rows = w.map(function (o) {
      var c = cust(o.custId), t = linesTotal(o.lines);
      return '<div class="list-row"><input type="checkbox" data-act="pick-order" data-id="' + o.id + '" ' + (ui.picked[o.id] ? 'checked' : '') + ' aria-label="Select ' + o.no + '"><div class="grow"><b>' + esc(c.name) + '</b> <span class="faint">· ' + o.no + '</span><div class="muted mini">' + o.lines.map(function (l) { return l.qty + ' ' + esc(prod(l.pid).name); }).join(', ') + '</div></div><div class="faint mini">' + dt(o.date) + ' · ' + o.staff + '</div><b class="num">' + money(t) + '</b><button class="btn btn-s" data-act="invoice-one" data-id="' + o.id + '">Invoice</button></div>';
    }).join('');
    var n = Object.keys(ui.picked).filter(function (k) { return ui.picked[k]; }).length;
    return head('Orders to invoice', 'Turn waiting orders into invoices in one tap.', '<a class="btn btn-p" href="#/orders/new">' + icon('plus', 18, 2.2) + 'New order</a>') +
      '<div class="card flush"><div class="list-head"><label class="check"><input type="checkbox" data-act="pick-all" ' + (w.length && n === w.length ? 'checked' : '') + '> Select all</label><span class="grow"></span><span class="muted">' + w.length + ' waiting · ' + money(sum(w, function (o) { return linesTotal(o.lines); })) + '</span></div>' + (rows || '<div class="empty">All caught up. Nothing is waiting for an invoice.</div>') + '</div>' +
      (w.length ? '<div class="actionbar"><span>' + (n ? n + ' selected' : 'Select orders to invoice together') + '</span><button class="btn btn-p ' + (n ? '' : 'is-off') + '" data-act="invoice-selected">Create ' + (n || '') + ' invoice' + (n === 1 ? '' : 's') + '</button></div>' : '');
  }

  function pInvoices() {
    var all = state.invoices.slice().sort(function (a, b) { return b.id - a.id; });
    var cnt = { all: all.length, unpaid: 0, partial: 0, paid: 0 }; all.forEach(function (i) { cnt[invStatus(i)]++; });
    var q = ui.invQ.trim().toLowerCase();
    var list = all.filter(function (i) { return (ui.invStatus === 'all' || invStatus(i) === ui.invStatus) && (!q || (i.no + ' ' + cust(i.custId).name).toLowerCase().indexOf(q) >= 0); });
    var billed = sum(list, invTotal), paid = sum(list, invPaid), per = 10;
    ui.invPage = Math.min(ui.invPage, Math.max(1, Math.ceil(list.length / per)));
    var page = list.slice((ui.invPage - 1) * per, ui.invPage * per);
    var pill = function (s) { return s === 'paid' ? '<span class="pill good">Paid</span>' : s === 'partial' ? '<span class="pill warn">Partly paid</span>' : '<span class="pill bad">Unpaid</span>'; };
    return head('Invoices', 'Open an invoice to collect a payment against it.') +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Billed</div><div class="v">' + money(billed) + '</div></div><div class="card kpi small"><div class="lbl">Collected</div><div class="v">' + money(paid) + '</div><div class="meter good"><i style="width:' + (billed ? Math.round(paid / billed * 100) : 0) + '%"></i></div></div><div class="card kpi small alert"><div class="lbl">Still to collect</div><div class="v" style="color:#B3231C">' + money(billed - paid) + '</div></div></section>' +
      '<div class="row between wrap" style="gap:12px"><div class="seg" role="group" aria-label="Payment status">' + [['all', 'All'], ['unpaid', 'Unpaid'], ['partial', 'Partly paid'], ['paid', 'Paid']].map(function (x) { return '<button class="' + (ui.invStatus === x[0] ? 'on' : '') + '" data-act="inv-status" data-v="' + x[0] + '">' + x[1] + ' <span class="faint">' + cnt[x[0]] + '</span></button>'; }).join('') + '</div><input type="search" id="invQ" class="search" placeholder="Search invoice or customer" value="' + esc(ui.invQ) + '" aria-label="Search invoices"></div>' +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Invoice</th><th>Date</th><th>Customer</th><th class="r">Total</th><th class="r">Paid</th><th class="r">Due</th><th>Status</th></tr></thead><tbody>' +
      (page.map(function (i) { return '<tr class="click" data-go="#/invoices/' + i.id + '"><td><a class="a-link" href="#/invoices/' + i.id + '">' + i.no + '</a></td><td>' + dt(i.date) + '</td><td>' + esc(cust(i.custId).name) + '</td><td class="r num">' + money(invTotal(i)) + '</td><td class="r num">' + money(invPaid(i)) + '</td><td class="r num strong">' + money(invDue(i)) + '</td><td>' + pill(invStatus(i)) + '</td></tr>'; }).join('') || '<tr><td colspan="7" class="empty">No invoices match.</td></tr>') +
      '</tbody></table></div>' + pager(ui.invPage, list.length, per, 'inv-page') + '</div>';
  }

  function pInvoice(id) {
    var inv = state.invoices.filter(function (i) { return i.id === id; })[0];
    if (!inv) return head('Invoice not found', '<a class="a-link" href="#/invoices">Back to invoices</a>');
    var c = cust(inv.custId), due = invDue(inv), cols = state.collections.filter(function (x) { return x.invId === inv.id; });
    var sheet = '<div class="sheet"><div class="row between top"><div><h2>Invoice ' + inv.no + '</h2><div class="muted">' + dt(inv.date) + ' · Sales: ' + inv.staff + '</div></div><img src="img/logo-purple.jpg" alt="Talukder Foods" style="width:92px;border-radius:10px"></div>' +
      '<div><div class="sec-label">Billed to</div><b>' + esc(c.name) + '</b><div class="muted">' + esc(c.owner) + ' · ' + esc(c.area) + ' · ' + esc(c.phone) + '</div></div>' +
      '<table><thead><tr><th>Item</th><th class="r">Qty</th><th class="r">Price</th><th class="r">Amount</th></tr></thead><tbody>' + inv.lines.map(function (l) { return '<tr><td>' + esc(prod(l.pid).name) + '</td><td class="r">' + l.qty + '</td><td class="r">' + money(l.price) + '</td><td class="r">' + money(l.qty * l.price) + '</td></tr>'; }).join('') + '</tbody></table>' +
      '<div class="col" style="align-items:flex-end;gap:6px">' + (inv.discount ? '<div>Discount <b class="num">− ' + money(inv.discount) + '</b></div>' : '') + '<div>Total <b class="num" style="font-size:20px">' + money(invTotal(inv)) + '</b></div><div class="muted">Paid ' + money(invPaid(inv)) + '</div><div>Due <b class="num" style="font-size:20px;color:' + (due ? '#B3231C' : '#0B5F59') + '">' + money(due) + '</b></div></div></div>';
    var panel = due > 0 ?
      '<aside class="card panel"><div class="ct">Collect payment</div><div class="lbl">Due on this invoice: <b class="num">' + money(due) + '</b></div><div class="field"><label for="amt">Amount received</label><div style="position:relative"><span style="position:absolute;left:12px;top:14px;font-size:22px;font-weight:600;color:var(--red)">৳</span><input id="amt" class="amount-input" type="number" min="1" max="' + due + '" value="' + due + '" inputmode="numeric"></div></div>' +
      '<div class="field"><span class="fl">Method</span><div class="seg" id="method">' + ['Cash', 'bKash', 'Bank'].map(function (m, k) { return '<button type="button" class="' + (k === 0 ? 'on' : '') + '" data-act="method" data-v="' + m + '">' + m + '</button>'; }).join('') + '</div></div>' +
      '<button class="btn btn-p btn-lg btn-block" data-act="collect" data-id="' + inv.id + '">Record payment</button></aside>' :
      '<aside class="card panel"><div class="banner"><b>Fully paid</b></div>' + (cols.length ? '<div class="sec-label">Payments</div>' + cols.map(function (x) { return '<div class="row between"><span>' + dt(x.date) + ' · ' + x.method + '</span><b class="num">' + money(x.amount) + '</b></div>'; }).join('') : '') + '</aside>';
    return head('Invoice ' + inv.no, '<a class="a-link" href="#/invoices">&larr; All invoices</a>', '<button class="btn" data-act="print">' + icon('print', 18) + 'Print</button>') + '<div class="flow top"><div class="grow" style="flex:2 1 560px">' + sheet + '</div>' + panel + '</div>';
  }

  function pCollections() {
    var list = state.collections.slice().sort(function (a, b) { return b.id - a.id; }), per = 10;
    ui.colPage = Math.min(ui.colPage, Math.max(1, Math.ceil(list.length / per)));
    var page = list.slice((ui.colPage - 1) * per, ui.colPage * per), t = isoOf(0);
    var todays = sum(list.filter(function (c) { return c.date === t; }), function (c) { return c.amount; });
    return head('Collections', 'Every payment received, newest first.') +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Collected today</div><div class="v">' + money(todays) + '</div></div><div class="card kpi small"><div class="lbl">Last 30 days</div><div class="v">' + money(sum(list, function (c) { return c.amount; })) + '</div></div><div class="card kpi small"><div class="lbl">Payments</div><div class="v">' + list.length + '</div></div></section>' +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Date</th><th>Customer</th><th>Invoice</th><th>Method</th><th class="r">Amount</th></tr></thead><tbody>' +
      page.map(function (c) { var i = state.invoices.filter(function (x) { return x.id === c.invId; })[0]; return '<tr><td>' + dt(c.date) + '</td><td>' + esc(cust(c.custId).name) + '</td><td><a class="a-link" href="#/invoices/' + c.invId + '">' + (i ? i.no : '') + '</a></td><td><span class="pill neutral">' + c.method + '</span></td><td class="r num strong">' + money(c.amount) + '</td></tr>'; }).join('') +
      '</tbody></table></div>' + pager(ui.colPage, list.length, per, 'col-page') + '</div>';
  }

  function pProducts() {
    return head('Products', state.products.length + ' items in the range.') +
      '<div class="product-grid">' + state.products.map(function (p) { var c = cat(p.cat), sold = sum(state.invoices, function (i) { return sum(i.lines.filter(function (l) { return l.pid === p.id; }), function (l) { return l.qty; }); }); return '<div class="product"><div class="row"><img class="thumb" src="img/' + p.img + '.jpg" alt=""><div class="grow"><b>' + esc(p.name) + '</b><div><span class="pill neutral">' + c.name + '</span></div></div></div><div class="row between"><span class="num strong" style="font-size:18px">' + money(p.price) + '</span><span class="muted mini">' + sold + ' sold in 30 days</span></div></div>'; }).join('') + '</div>';
  }

  function pCustomers() {
    var rows = state.customers.map(function (c) { var due = custDue(c.id), use = Math.min(100, Math.round(due / c.limit * 100)); return '<tr class="click" data-go="#/customers/' + c.id + '"><td><div class="row"><div class="avatar-s">' + initials(c.name) + '</div><div><b>' + esc(c.name) + '</b><div class="muted mini">' + esc(c.owner) + '</div></div></div></td><td>' + esc(c.area) + '</td><td class="r num">' + money(custBilled(c.id)) + '</td><td class="r num strong">' + money(due) + '</td><td style="min-width:150px"><div class="meter ' + (use >= 80 ? '' : 'good') + '"><i style="width:' + use + '%;' + (use >= 80 ? 'background:#E5412D' : '') + '"></i></div><div class="muted mini">' + use + '% of ' + money(c.limit) + '</div></td></tr>'; }).join('');
    return head('Customers', 'Shops you sell to, what they owe and how close they are to their credit limit.') +
      '<div class="card flush"><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Customer</th><th>Area</th><th class="r">Billed (30d)</th><th class="r">Owes</th><th>Credit used</th></tr></thead><tbody>' + rows + '</tbody></table></div></div>';
  }

  function pCustomer(id) {
    var c = cust(id); if (!c) return head('Customer not found');
    var invs = state.invoices.filter(function (i) { return i.custId === id; }).sort(function (a, b) { return b.id - a.id; });
    return head(esc(c.name), esc(c.owner) + ' · ' + esc(c.area) + ' · ' + esc(c.phone) + ' · <a class="a-link" href="#/customers">All customers</a>', '<a class="btn btn-p" data-act="order-for" data-id="' + id + '">' + icon('plus', 18, 2.2) + 'New order</a>') +
      '<section class="kpis"><div class="card kpi small"><div class="lbl">Billed (30 days)</div><div class="v">' + money(custBilled(id)) + '</div></div><div class="card kpi small alert"><div class="lbl">Owes now</div><div class="v" style="color:#B3231C">' + money(custDue(id)) + '</div></div><div class="card kpi small"><div class="lbl">Credit limit</div><div class="v">' + money(c.limit) + '</div></div></section>' +
      '<div class="card flush"><div class="ct" style="padding:18px 20px 6px">Statement</div><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Invoice</th><th>Date</th><th class="r">Total</th><th class="r">Paid</th><th class="r">Due</th></tr></thead><tbody>' + invs.map(function (i) { return '<tr class="click" data-go="#/invoices/' + i.id + '"><td><a class="a-link" href="#/invoices/' + i.id + '">' + i.no + '</a></td><td>' + dt(i.date) + '</td><td class="r num">' + money(invTotal(i)) + '</td><td class="r num">' + money(invPaid(i)) + '</td><td class="r num strong">' + money(invDue(i)) + '</td></tr>'; }).join('') + '</tbody></table></div></div>';
  }

  // ---------- login ----------
  function loginPage() {
    return '<div class="auth"><div class="auth-hero"><div><img src="img/logo-yellow.png" alt="Talukder Foods"></div></div><main class="auth-form"><form id="loginForm"><div><h1 class="page-title" style="font-size:34px">Welcome back</h1><div class="page-sub">Sign in to take orders, invoice and collect.</div></div>' +
      '<div class="auth-hint"><b>Live demo.</b> Sample data, no server. Just press Sign in.</div>' +
      '<div class="field"><label for="u">Username</label><input id="u" value="admin" autocomplete="username"></div><div class="field"><label for="p">Password</label><input id="p" type="password" value="demo1234" autocomplete="current-password"></div>' +
      '<button class="btn btn-p btn-lg btn-block" type="submit">Sign in</button></form></main></div>';
  }

  // ---------- router ----------
  function route() {
    var h = location.hash.replace(/^#/, '') || '/dashboard', m;
    if (h === '/dashboard') return { nav: '#/dashboard', title: 'Dashboard', html: pDashboard };
    if (h === '/reports') return { nav: '#/reports', title: 'KPI report', html: pReports };
    if (h === '/orders/new') return { nav: '#/orders/new', title: 'New order', html: pNewOrder };
    if (h === '/orders') return { nav: '#/orders', title: 'Orders to invoice', html: pOrders };
    if (h === '/invoices') return { nav: '#/invoices', title: 'Invoices', html: pInvoices };
    if ((m = h.match(/^\/invoices\/(\d+)$/))) return { nav: '#/invoices', title: 'Invoice', html: function () { return pInvoice(+m[1]); } };
    if (h === '/collections') return { nav: '#/collections', title: 'Collections', html: pCollections };
    if (h === '/products') return { nav: '#/products', title: 'Products', html: pProducts };
    if (h === '/customers') return { nav: '#/customers', title: 'Customers', html: pCustomers };
    if ((m = h.match(/^\/customers\/(\d+)$/))) return { nav: '#/customers', title: 'Customer', html: function () { return pCustomer(+m[1]); } };
    return { nav: '#/dashboard', title: 'Dashboard', html: pDashboard };
  }

  function render(keepScroll) {
    var y = window.scrollY, app = $('#app');
    if (!sessionStorage.getItem(SESSION)) { app.innerHTML = loginPage(); document.title = 'Sign in · Talukder Foods Sales demo'; return; }
    var r = route(); app.innerHTML = layout(r, r.html()); document.title = r.title + ' · Talukder Foods Sales demo';
    window.scrollTo(0, keepScroll ? y : 0);
  }

  // ---------- actions ----------
  function nextNo(prefix, key) { state.seq[key]++; return prefix + (key === 'inv' ? 1000 + state.seq[key] : 200 + state.seq[key]); }
  function makeInvoice(o) {
    var id = state.seq.inv + 1, no = nextNo('INV-', 'inv');
    state.invoices.push({ id: id, no: no, date: today(), custId: o.custId, staff: o.staff, lines: o.lines, discount: 0 });
    o.status = 'invoiced'; return id;
  }

  document.addEventListener('click', function (e) {
    var go = e.target.closest('[data-go]'); if (go && !e.target.closest('a')) { location.hash = go.dataset.go; return; }
    var t = e.target.closest('[data-act]'); if (!t) return;
    var a = t.dataset.act, id = +t.dataset.id;
    switch (a) {
      case 'logout': sessionStorage.removeItem(SESSION); render(); break;
      case 'reset': if (confirm('Reset the demo to its original sample data?')) { state = D.seed(); save(); ui.cart = { custId: null, lines: {} }; ui.picked = {}; toast('Demo data reset'); render(true); } break;
      case 'period': ui.period = t.dataset.v; render(true); break;
      case 'pick-cust': ui.cart.custId = id; render(true); break;
      case 'change-cust': ui.cart.custId = null; ui.custQ = ''; render(true); break;
      case 'prodcat': ui.prodCat = t.dataset.v; render(true); break;
      case 'step': var cur = ui.cart.lines[id] || 0; ui.cart.lines[id] = Math.max(0, cur + (+t.dataset.d)); render(true); break;
      case 'rm': delete ui.cart.lines[id]; render(true); break;
      case 'place':
        var c = ui.cart, ls = Object.keys(c.lines).filter(function (k) { return c.lines[k] > 0; }).map(function (k) { return { pid: +k, qty: c.lines[k], price: prod(+k).price }; });
        if (!c.custId || !ls.length) return;
        var oid = state.orders.length ? Math.max.apply(null, state.orders.map(function (x) { return x.id; })) + 1 : 1;
        state.orders.push({ id: oid, no: nextNo('ORD-', 'ord'), date: today(), custId: c.custId, staff: 'Sabbir', status: 'waiting', lines: ls });
        save(); toast('Order placed for ' + cust(c.custId).name); ui.cart = { custId: null, lines: {} }; location.hash = '#/orders'; break;
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
        state.collections.push({ id: ++state.seq.col, date: today(), invId: inv.id, custId: inv.custId, amount: amt, method: ($('#method .on') || {}).dataset ? $('#method .on').dataset.v : 'Cash' });
        save(); toast(money(amt) + ' recorded for ' + inv.no); render(true); break;
      case 'print': window.print(); break;
      case 'order-for': ui.cart = { custId: id, lines: {} }; location.hash = '#/orders/new'; break;
    }
  });

  document.addEventListener('input', function (e) {
    if (e.target.id === 'custQ') { ui.custQ = e.target.value; $('#custResults').innerHTML = custResults(); }
    if (e.target.id === 'invQ') { ui.invQ = e.target.value; ui.invPage = 1; var pos = e.target.selectionStart; render(true); var n = $('#invQ'); if (n) { n.focus(); n.setSelectionRange(pos, pos); } }
  });

  document.addEventListener('submit', function (e) {
    if (e.target.id === 'loginForm') { e.preventDefault(); sessionStorage.setItem(SESSION, '1'); if (!location.hash) location.hash = '#/dashboard'; render(); }
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
  render();
})();
