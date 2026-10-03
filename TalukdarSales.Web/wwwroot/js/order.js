// New order: customer picker, product cards, live cart and credit warning. The server only sees UserId and Qty[id].
(function () {
  const form = document.getElementById('order');
  if (!form) return;
  const $ = (s, r) => (r || document).querySelector(s);
  const $$ = (s, r) => Array.from((r || document).querySelectorAll(s));
  const ind = (n) => {
    n = Math.round(n * 100) / 100;
    const neg = n < 0; let [w, f] = Math.abs(n).toFixed(2).split('.');
    const last = w.slice(-3), rest = w.slice(0, -3).replace(/\B(?=(\d{2})+(?!\d))/g, ',');
    w = rest ? rest + ',' + last : last;
    return (neg ? '-' : '') + w + (f === '00' ? '' : '.' + f);
  };
  const userId = $('#UserId'), search = $('#cust-search'), results = $('#cust-results'), none = $('#cust-none');
  let cust = null;

  function pick(btn) {
    cust = { id: btn.dataset.id, name: btn.dataset.name, code: btn.dataset.code, phone: btn.dataset.phone,
      due: parseFloat(btn.dataset.due) || 0, limit: parseFloat(btn.dataset.limit) || 0, days: btn.dataset.days };
    userId.value = cust.id;
    results.hidden = true; search.value = '';
    $$('.chip.cust').forEach(c => c.classList.toggle('on', c.dataset.id === cust.id));
    $('#cc-initials').textContent = cust.name.split(' ').filter(Boolean).slice(0, 2).map(x => x[0].toUpperCase()).join('');
    $('#cc-name').textContent = cust.name;
    $('#cc-meta').textContent = 'ID ' + cust.code + ' · ' + cust.phone + (cust.days > 0 ? ' · ' + cust.days + ' credit days' : '');
    $('#cust-card').hidden = false;
    refresh();
  }
  $$('.cust').forEach(b => b.addEventListener('click', () => pick(b)));
  search.addEventListener('input', () => {
    const q = search.value.trim().toLowerCase();
    results.hidden = !q;
    let shown = 0;
    $$('.cust', results).forEach(b => { const hit = !q || b.dataset.q.includes(q); b.hidden = !hit; if (hit) shown++; });
    none.hidden = shown > 0;
  });

  // products
  const products = $$('.product');
  function setQty(p, q) { q = Math.max(0, q); $('.qty', p).value = q; refresh(); }
  products.forEach(p => {
    const input = $('.qty', p);
    $('.add', p).addEventListener('click', () => setQty(p, 1));
    $('.add12', p).addEventListener('click', () => setQty(p, (parseFloat(input.value) || 0) + 12));
    $('.inc', p).addEventListener('click', () => setQty(p, (parseFloat(input.value) || 0) + 1));
    $('.dec', p).addEventListener('click', () => setQty(p, (parseFloat(input.value) || 0) - 1));
    input.addEventListener('input', refresh);
  });
  let cat = '';
  function filter() {
    const q = $('#prod-search').value.trim().toLowerCase(); let shown = 0;
    products.forEach(p => { const hit = (!cat || p.dataset.cat === cat) && (!q || p.dataset.name.toLowerCase().includes(q)); p.hidden = !hit; if (hit) shown++; });
    $('#prod-none').hidden = shown > 0;
  }
  $('#prod-search').addEventListener('input', filter);
  $$('.cat').forEach(c => c.addEventListener('click', () => { cat = c.dataset.cat; $$('.cat').forEach(x => x.classList.toggle('on', x === c)); filter(); }));

  function refresh() {
    let total = 0, items = 0; const lines = [];
    products.forEach(p => {
      const q = parseFloat($('.qty', p).value) || 0, price = parseFloat(p.dataset.price);
      p.classList.toggle('has', q > 0);
      $('.add', p).hidden = q > 0; $('.stepper', p).hidden = !(q > 0);
      if (q > 0) { total += q * price; items++; lines.push({ p, q, price }); }
    });
    const box = $('#cart-lines'); box.textContent = '';
    lines.forEach(l => {
      const row = document.createElement('div'); row.className = 'cart-line';
      row.innerHTML = '<div class="grow" style="min-width:0"><div style="font-weight:500"></div><div class="lbl"></div></div><div class="num strong"></div><button type="button" class="rm" aria-label="Remove">×</button>';
      row.children[0].children[0].textContent = l.p.dataset.name;
      row.children[0].children[1].textContent = ind(l.q) + ' × ৳ ' + ind(l.price);
      row.children[1].textContent = '৳ ' + ind(l.q * l.price);
      row.children[2].addEventListener('click', () => setQty(l.p, 0));
      box.appendChild(row);
    });
    $('#cart-empty').hidden = items > 0;
    $('#cart-count').textContent = items + (items === 1 ? ' item' : ' items');
    $('#cart-total').textContent = '৳ ' + ind(total);
    $('#mb-count').textContent = items + (items === 1 ? ' item' : ' items'); $('#mb-total').textContent = '৳ ' + ind(total);
    $('#mobilebar').hidden = items === 0;

    const warn = $('#cart-warn'); warn.hidden = true; warn.className = 'note';
    if (cust) {
      const limit = cust.limit;
      $('#cc-credit').textContent = '৳ ' + ind(cust.due + total) + (limit > 0 ? ' of ৳ ' + ind(limit) : '');
      const base = Math.max(limit, cust.due + total, 1);
      $('#cc-due').style.width = (cust.due / base * 100) + '%';
      $('#cc-order').style.width = (total / base * 100) + '%';
      if (limit > 0 && cust.due + total > limit) { warn.hidden = false; warn.classList.add('bad'); warn.textContent = 'Over the credit limit by ৳ ' + ind(cust.due + total - limit) + '. You can still send it; collect payment soon.'; }
    }
    const ready = !!cust && items > 0;
    $('#send').disabled = !ready;
    $('#send-hint').textContent = ready ? 'Goes to Orders to invoice.' : (!cust ? 'Choose a customer and at least one product.' : 'Add at least one product.');
  }
  // restore customer after a failed post
  if (userId.value) { const b = $$('.cust').find(x => x.dataset.id === userId.value); if (b) pick(b); }
  refresh();
})();
