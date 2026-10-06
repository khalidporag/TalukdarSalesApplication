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
    return { v: 1, customers: CUSTOMERS, products: PRODUCTS, invoices: invoices, collections: collections, orders: orders, seq: { inv: n, col: c, ord: orders.length } };
  }

  window.DEMO = { CATS: CATS, STAFF: STAFF, seed: seed, day: day };
})();
