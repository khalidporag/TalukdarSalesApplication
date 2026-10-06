/* Sample data for the static demo. All names, numbers and dates are made up. */
(function () {
  'use strict';

  var CATS = [
    { id: 'bread', name: 'Bread', cls: 'cat-1', color: '#E5412D' },
    { id: 'biscuit', name: 'Biscuits & cookies', cls: 'cat-3', color: '#C98A12' },
    { id: 'snack', name: 'Snacks', cls: 'cat-4', color: '#1F9E96' },
    { id: 'sweet', name: 'Sweets', cls: 'cat-2', color: '#7B4BB8' },
    { id: 'cake', name: 'Cakes', cls: 'cat-0', color: '#8F8294' }
  ];

  var PRODUCTS = [
    { id: 1, name: 'Premium Bread', cat: 'bread', price: 70, img: 'bread-purple' },
    { id: 2, name: 'Family Bread', cat: 'bread', price: 110, img: 'bread-ramadan' },
    { id: 3, name: 'Tea-time Toast', cat: 'biscuit', price: 60, img: 'toast-blue' },
    { id: 4, name: 'Premium Toast', cat: 'biscuit', price: 90, img: 'toast-tea' },
    { id: 5, name: 'Fair Break Cookies', cat: 'biscuit', price: 320, img: 'fairbreak-green' },
    { id: 6, name: 'Coconut Cookies Box', cat: 'biscuit', price: 380, img: 'fairbreak-coconut' },
    { id: 7, name: 'Chira Vhaja', cat: 'snack', price: 80, img: 'chira' },
    { id: 8, name: 'Nimok Para', cat: 'snack', price: 120, img: 'nimok' },
    { id: 9, name: 'Chanachur', cat: 'snack', price: 100, img: 'chanachur' },
    { id: 10, name: 'Lachcha Semai', cat: 'sweet', price: 60, img: 'semai-red' },
    { id: 11, name: 'Special Semai Gift Bag', cat: 'sweet', price: 180, img: 'semai-family' },
    { id: 12, name: 'Cheesecake Slice', cat: 'cake', price: 160, img: 'cheesecake' }
  ];

  var CUSTOMERS = [
    { id: 1, name: 'Rahim Store', owner: 'Abdur Rahim', area: 'Mirpur 10', phone: '01711-000101', limit: 60000 },
    { id: 2, name: 'Karim Traders', owner: 'Abdul Karim', area: 'Uttara Sector 7', phone: '01711-000102', limit: 80000 },
    { id: 3, name: 'Sultana Mart', owner: 'Sultana Begum', area: 'Dhanmondi 27', phone: '01711-000103', limit: 50000 },
    { id: 4, name: 'New Dhaka Bakery', owner: 'Mizanur Rahman', area: 'Old Dhaka', phone: '01711-000104', limit: 70000 },
    { id: 5, name: 'Green Valley Super Shop', owner: 'Tanvir Ahmed', area: 'Bashundhara', phone: '01711-000105', limit: 100000 },
    { id: 6, name: 'Hasan & Sons', owner: 'Hasan Mahmud', area: 'Mohammadpur', phone: '01711-000106', limit: 45000 },
    { id: 7, name: 'Moni Confectionery', owner: 'Moni Akter', area: 'Rampura', phone: '01711-000107', limit: 40000 },
    { id: 8, name: 'City Corner', owner: 'Shafiq Islam', area: 'Gulshan 1', phone: '01711-000108', limit: 90000 },
    { id: 9, name: 'Nabila Enterprise', owner: 'Nabila Khan', area: 'Badda', phone: '01711-000109', limit: 35000 },
    { id: 10, name: 'Bismillah Store', owner: 'Yusuf Ali', area: 'Jatrabari', phone: '01711-000110', limit: 30000 }
  ];

  var STAFF = ['Sabbir', 'Nusrat', 'Imran'];

  var PERMS = [
    ['dashboard', 'See the dashboard', 'Overview'],
    ['reports', 'See the KPI report', 'Overview'],
    ['notices', 'Post and remove notices', 'Overview'],
    ['order.create', 'Take new orders', 'Sell'],
    ['order.view', 'See orders waiting for an invoice', 'Sell'],
    ['order.manage', 'Edit and cancel orders, create invoices', 'Sell'],
    ['invoice.view', 'See invoices', 'Sell'],
    ['invoice.collect', 'Collect payments', 'Sell'],
    ['invoice.discount', 'Give discounts on invoices', 'Sell'],
    ['invoice.return', 'Record returns and credit notes', 'Sell'],
    ['collection.view', 'See collections', 'Sell'],
    ['production', 'See the production plan', 'Operate'],
    ['products', 'Manage products and categories', 'Operate'],
    ['customers', 'Manage customers', 'Operate'],
    ['users', 'Manage users', 'Admin'],
    ['roles', 'Manage roles and permissions', 'Admin'],
    ['window', 'Change the order window', 'Admin'],
    ['audit', 'See the audit log', 'Admin']
  ];

  function rng(seed) {
    return function () {
      seed |= 0; seed = seed + 0x6D2B79F5 | 0;
      var t = Math.imul(seed ^ seed >>> 15, 1 | seed);
      t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t;
      return ((t ^ t >>> 14) >>> 0) / 4294967296;
    };
  }

  function day(offset) {
    var d = new Date(); d.setHours(0, 0, 0, 0); d.setDate(d.getDate() - offset);
    return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
  }

  function seed() {
    var r = rng(11);
    var pick = function (a) { return a[Math.floor(r() * a.length)]; };
    var price = {}; PRODUCTS.forEach(function (p) { price[p.id] = p.price; });
    var invoices = [], collections = [], n = 0, c = 0;
    for (var off = 29; off >= 0; off--) {
      var date = day(off);
      var dow = new Date(date + 'T00:00:00').getDay();
      var count = dow === 5 ? 1 + Math.floor(r() * 2) : 2 + Math.floor(r() * 3);
      for (var k = 0; k < count; k++) {
        var cust = CUSTOMERS[Math.floor(Math.pow(r(), 1.4) * CUSTOMERS.length)];
        var lines = [], used = {}, nl = 2 + Math.floor(r() * 4);
        for (var j = 0; j < nl; j++) {
          var p = pick(PRODUCTS); if (used[p.id]) continue; used[p.id] = 1;
          lines.push({ pid: p.id, qty: 5 + Math.floor(r() * 36), price: price[p.id] });
        }
        var inv = { id: ++n, no: 'INV-' + String(1000 + n), date: date, custId: cust.id, staff: pick(STAFF), lines: lines, discount: r() < 0.12 ? 100 * (1 + Math.floor(r() * 5)) : 0 };
        var total = lines.reduce(function (s, l) { return s + l.qty * l.price; }, 0) - inv.discount;
        var roll = r(), paid = 0;
        if (off > 3) paid = roll < 0.7 ? total : roll < 0.9 ? Math.round(total * (0.4 + r() * 0.4) / 10) * 10 : 0;
        else paid = roll < 0.3 ? total : roll < 0.45 ? Math.round(total * 0.5 / 10) * 10 : 0;
        if (paid > 0) {
          collections.push({ id: ++c, date: day(Math.max(0, off - Math.floor(r() * 4))), invId: inv.id, custId: cust.id, amount: paid, method: pick(['Cash', 'bKash', 'Bank']) });
        }
        invoices.push(inv);
      }
    }
    var orders = [];
    [[1, [[5, 12], [3, 20]]], [4, [[1, 30], [2, 15]]], [8, [[6, 8], [11, 10], [9, 25]]], [2, [[7, 18], [8, 14]]], [6, [[10, 40]]]].forEach(function (o, i) {
      orders.push({ id: i + 1, no: 'ORD-' + (200 + i), date: day(i < 3 ? 0 : 1), custId: o[0], staff: STAFF[i % 3], status: 'waiting', lines: o[1].map(function (x) { return { pid: x[0], qty: x[1], price: price[x[0]] }; }) });
    });
    var cats = CATS.map(function (c, i) { return { id: c.id, name: c.name, cls: c.cls, color: c.color }; });
    var all = PERMS.map(function (p) { return p[0]; });
    var roles = [
      { id: 1, name: 'Administrator', locked: true, perms: all },
      { id: 2, name: 'Sales manager', perms: all.filter(function (k) { return ['users', 'roles', 'window', 'audit'].indexOf(k) < 0; }) },
      { id: 3, name: 'Salesperson', perms: ['order.create', 'order.view', 'invoice.view', 'production'] },
      { id: 4, name: 'Accountant', perms: ['dashboard', 'reports', 'invoice.view', 'invoice.collect', 'invoice.discount', 'invoice.return', 'collection.view', 'customers'] }
    ];
    var users = [
      { id: 1, name: 'Demo Admin', username: 'admin', role: 'Administrator', active: true, pass: 'demo1234' },
      { id: 2, name: 'Sabbir Hossain', username: 'sabbir', role: 'Salesperson', active: true, pass: 'demo1234' },
      { id: 3, name: 'Nusrat Jahan', username: 'nusrat', role: 'Salesperson', active: true, pass: 'demo1234' },
      { id: 4, name: 'Imran Khan', username: 'imran', role: 'Salesperson', active: true, pass: 'demo1234' },
      { id: 5, name: 'Farhana Akter', username: 'farhana', role: 'Accountant', active: true, pass: 'demo1234' },
      { id: 6, name: 'Tahmina Sultana', username: 'tahmina', role: 'Sales manager', active: true, pass: 'demo1234' }
    ];
    var notices = [
      { id: 1, date: day(0), title: 'Order desk closes at 10 PM', body: 'Orders placed after the closing time are picked up the next morning. Please finish your rounds early on Thursdays.', by: 'Demo Admin' },
      { id: 2, date: day(2), title: 'New: Special Semai Gift Bag', body: 'The gift bag is now in the product list at ৳ 180. Stock is limited for the first week.', by: 'Tahmina Sultana' },
      { id: 3, date: day(6), title: 'Collect before month end', body: 'Please clear invoices older than 15 days before the month closes. Check the "Who owes how long" card on the KPI report.', by: 'Demo Admin' }
    ];
    var notifications = [
      { id: 1, time: day(0) + 'T08:05:00', title: 'Order desk is open', body: 'Orders are open until 10:00 PM today.', read: false },
      { id: 2, time: day(0) + 'T07:30:00', title: '5 orders are waiting for an invoice', body: 'Open Orders to invoice to turn them into invoices.', read: false },
      { id: 3, time: day(1) + 'T21:00:00', title: 'Closing reminder', body: 'The order desk closes in one hour.', read: true }
    ];
    var audit = [];
    var acts = [['Order placed', 'ORD-198 for Rahim Store'], ['Invoice created', 'INV-1' + '085 for Karim Traders'], ['Payment recorded', '৳ 4,200 against INV-1081'], ['Product added', 'Special Semai Gift Bag'], ['User added', 'tahmina (Sales manager)'], ['Discount given', '৳ 200 on INV-1077'], ['Order window changed', 'Closing time set to 10:00 PM'], ['Role changed', 'Accountant: collect payments on']];
    acts.forEach(function (a, i) { audit.push({ id: i + 1, time: day(7 - i) + 'T' + ['09:12', '10:40', '11:05', '14:20', '15:45', '12:30', '16:10', '17:02'][i] + ':00', user: i % 2 ? 'Tahmina Sultana' : 'Demo Admin', action: a[0], detail: a[1] }); });
    return { v: 2, customers: CUSTOMERS, products: PRODUCTS, categories: cats, invoices: invoices, collections: collections, orders: orders, returns: [], users: users, roles: roles, notices: notices, notifications: notifications, audit: audit, window: { enforce: true, open: '05:00', close: '22:00' }, seq: { inv: n, col: c, ord: orders.length, ret: 0, audit: audit.length, note: notifications.length, notice: notices.length, role: roles.length } };
  }

  window.DEMO = { CATS: CATS, PERMS: PERMS, STAFF: STAFF, seed: seed, day: day };
})();
