(function () {
  'use strict';

  // ---- Edit these ----
  var WHATSAPP = '8801700000000'; // international format, no + or spaces
  var PRODUCTS = [
    { id: 'oreo-cake', cat: 'Cakes', name: 'Oreo Chocolate Cake', img: 'oreo-cake.jpg' },
    { id: 'cheesecake', cat: 'Cakes', name: 'Cheesecake Slice', img: 'cheesecake.jpg' },
    { id: 'character-cake', cat: 'Cakes', name: 'Custom Character Cake', img: 'doraemon-cake.jpg' },
    { id: 'owl-cupcake', cat: 'Cakes', name: 'Chocolate Owl Cupcake', img: 'owl-cupcake.jpg' },
    { id: 'choc-chip', cat: 'Cookies', name: 'Choc Chip Cookies', img: 'cookies.jpg' },
    { id: 'fair-break', cat: 'Cookies', name: 'Fair Break Cookies', img: 'fairbreak-green.jpg' },
    { id: 'coconut-cookies', cat: 'Cookies', name: 'Coconut Cookies Box', img: 'fairbreak-coconut.jpg' },
    { id: 'premium-bread', cat: 'Bread & toast', name: 'Premium Bread', img: 'bread-purple.jpg' },
    { id: 'family-bread', cat: 'Bread & toast', name: 'Family Bread', img: 'bread-ramadan.jpg' },
    { id: 'toast-tea', cat: 'Bread & toast', name: 'Tea-time Toast', img: 'toast-blue.jpg' },
    { id: 'premium-toast', cat: 'Bread & toast', name: 'Premium Toast', img: 'toast-tea.jpg' },
    { id: 'lachcha-semai', cat: 'Snacks & sweets', name: 'Lachcha Semai', img: 'semai-red.jpg' },
    { id: 'premium-shemai', cat: 'Snacks & sweets', name: 'Premium Lachcha Shemai', img: 'semai-family.jpg' },
    { id: 'chira-vhaja', cat: 'Snacks & sweets', name: 'Chira Vhaja', img: 'chira.jpg' },
    { id: 'nimok-para', cat: 'Snacks & sweets', name: 'Nimok Para', img: 'nimok.jpg' },
    { id: 'chanachur', cat: 'Snacks & sweets', name: 'Chanachur', img: 'chanachur.jpg' }
  ];
  // --------------------

  var KEY = 'talukder-basket-v1';
  var $ = function (s) { return document.querySelector(s); };
  var byId = {};
  PRODUCTS.forEach(function (p) { byId[p.id] = p; });
  var cats = ['All'].concat(PRODUCTS.map(function (p) { return p.cat; }).filter(function (c, i, a) { return a.indexOf(c) === i; }));
  var cat = 'All';
  var basket = load();

  function load() {
    try { return JSON.parse(localStorage.getItem(KEY)) || {}; } catch (e) { return {}; }
  }
  function save() {
    try { localStorage.setItem(KEY, JSON.stringify(basket)); } catch (e) { /* ignore */ }
  }
  function totalQty() {
    return Object.keys(basket).reduce(function (n, k) { return n + basket[k]; }, 0);
  }
  function esc(s) {
    return String(s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; });
  }

  function renderCats() {
    $('#cats').innerHTML = cats.map(function (c) {
      return '<button class="pill" type="button" data-cat="' + esc(c) + '" aria-pressed="' + (c === cat) + '">' + esc(c) + '</button>';
    }).join('');
  }

  function renderProducts() {
    $('#products').innerHTML = PRODUCTS.filter(function (p) { return cat === 'All' || p.cat === cat; }).map(function (p) {
      var inBasket = basket[p.id] > 0;
      return '<article class="pcard"><img src="assets/img/' + p.img + '" alt="' + esc(p.name) + '" loading="lazy" width="1000" height="1000">' +
        '<div class="pbody"><h3>' + esc(p.name) + '</h3><p class="price">[৳ PRICE]</p>' +
        '<button class="btn btn-ink btn-sm add' + (inBasket ? ' added' : '') + '" type="button" data-add="' + p.id + '">' + (inBasket ? 'Added (' + basket[p.id] + ')' : 'Add to basket') + '</button></div></article>';
    }).join('');
  }

  function renderBasket() {
    var ids = Object.keys(basket).filter(function (k) { return basket[k] > 0 && byId[k]; });
    $('#count').textContent = totalQty();
    $('#totalQty').textContent = totalQty();
    $('#basketItems').innerHTML = ids.length ? ids.map(function (k) {
      var p = byId[k];
      return '<div class="line"><img src="assets/img/' + p.img + '" alt=""><div><b>' + esc(p.name) + '</b><span class="fine">[৳ PRICE]</span></div>' +
        '<div class="qty"><button type="button" data-dec="' + k + '" aria-label="Fewer ' + esc(p.name) + '">&minus;</button><span aria-live="polite">' + basket[k] + '</span><button type="button" data-inc="' + k + '" aria-label="More ' + esc(p.name) + '">+</button></div></div>';
    }).join('') : '<p class="empty">Your basket is empty. Add something sweet from the shop.</p>';
    $('#sendOrder').disabled = !ids.length;
    $('#sendOrder').style.opacity = ids.length ? '1' : '.5';
  }

  function change(id, d) {
    basket[id] = Math.max(0, (basket[id] || 0) + d);
    if (!basket[id]) delete basket[id];
    save(); renderBasket(); renderProducts();
  }

  // Drawer
  var drawer = $('#drawer'), scrim = $('#scrim'), lastFocus = null;
  function openBasket() {
    lastFocus = document.activeElement;
    drawer.classList.add('open'); drawer.setAttribute('aria-hidden', 'false'); scrim.hidden = false;
    document.body.style.overflow = 'hidden';
    $('#closeBasket').focus();
  }
  function closeBasket() {
    drawer.classList.remove('open'); drawer.setAttribute('aria-hidden', 'true'); scrim.hidden = true;
    document.body.style.overflow = '';
    if (lastFocus) lastFocus.focus();
  }
  function wa(text) {
    return 'https://wa.me/' + WHATSAPP + '?text=' + encodeURIComponent(text);
  }

  document.addEventListener('click', function (e) {
    var t = e.target.closest('[data-cat],[data-add],[data-inc],[data-dec]');
    if (!t) return;
    if (t.dataset.cat) { cat = t.dataset.cat; renderCats(); renderProducts(); }
    else if (t.dataset.add) { change(t.dataset.add, 1); }
    else if (t.dataset.inc) { change(t.dataset.inc, 1); }
    else if (t.dataset.dec) { change(t.dataset.dec, -1); }
  });
  $('#openBasket').addEventListener('click', openBasket);
  $('#closeBasket').addEventListener('click', closeBasket);
  scrim.addEventListener('click', closeBasket);
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && drawer.classList.contains('open')) closeBasket(); });

  $('#sendOrder').addEventListener('click', function () {
    var lines = Object.keys(basket).filter(function (k) { return byId[k]; }).map(function (k) { return '• ' + basket[k] + ' x ' + byId[k].name; });
    var msg = 'Hello Talukder Foods, I would like to order:\n' + lines.join('\n');
    var name = $('#oName').value.trim(), note = $('#oNote').value.trim();
    if (name) msg += '\n\nName: ' + name;
    if (note) msg += '\nAddress / note: ' + note;
    window.open(wa(msg), '_blank', 'noopener');
  });

  $('#enquiryForm').addEventListener('submit', function (e) {
    e.preventDefault();
    var f = e.target, name = f.name.value.trim(), date = f.date.value, idea = f.idea.value.trim();
    var err = $('#eErr');
    if (!name || !date || !idea) { err.hidden = false; return; }
    err.hidden = true;
    var msg = 'Hello Talukder Foods, custom cake enquiry:\nName: ' + name + '\nDate needed: ' + date + '\nSize: ' + f.size.value + '\nIdeas: ' + idea;
    window.open(wa(msg), '_blank', 'noopener');
  });

  $('#waLink').addEventListener('click', function (e) {
    e.preventDefault();
    window.open(wa('Hello Talukder Foods!'), '_blank', 'noopener');
  });

  var toTop = $('#toTop');
  window.addEventListener('scroll', function () { toTop.hidden = window.scrollY < 800; }, { passive: true });
  toTop.addEventListener('click', function () { window.scrollTo({ top: 0 }); });

  renderCats(); renderProducts(); renderBasket();
})();
