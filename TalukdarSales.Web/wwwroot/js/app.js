// Glue between htmx server events and the page: toasts, confirmations, small helpers.
(function () {
  const body = document.body;

  function toast(message, ok) {
    const box = document.getElementById('toasts');
    const el = document.createElement('div');
    el.className = 'toast' + (ok === false ? ' err' : '');
    el.innerHTML = '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="' +
      (ok === false ? 'M12 9v4M12 17h.01' : 'M5 12l5 5 9-10') + '"></path></svg><span></span>';
    el.querySelector('span').textContent = message || '';
    box.appendChild(el);
    setTimeout(() => el.remove(), 4500);
  }
  window.appToast = toast;

  // Server sends HX-Trigger: {"toast": {"message": "...", "success": true}}
  body.addEventListener('toast', (e) => { const d = e.detail || {}; toast(d.message, d.success !== false); });

  // 403 already carries a toast from the server; 401 is a redirect via HX-Redirect.
  body.addEventListener('htmx:responseError', (e) => {
    const s = e.detail.xhr.status;
    if (s === 401 || s === 403) return;
    toast('Something went wrong (' + s + '). Please try again.', false);
  });

  // <button data-confirm="Are you sure?" hx-post=...>
  body.addEventListener('htmx:confirm', (e) => {
    const msg = e.detail.elt.getAttribute('data-confirm');
    if (!msg) return;
    e.preventDefault();
    if (window.confirm(msg)) e.detail.issueRequest(true);
  });

  // Plain forms can ask for the same confirmation.
  document.addEventListener('submit', (e) => {
    const msg = e.target.getAttribute && e.target.getAttribute('data-confirm');
    if (msg && !window.confirm(msg)) e.preventDefault();
  });

  // Flash message passed through TempData
  const flash = document.querySelector('[data-flash]');
  if (flash) toast(flash.getAttribute('data-flash'), flash.getAttribute('data-ok') !== 'false');
})();

// Chart hover: server renders .hits cells carrying data-x (viewBox x), data-ys (circle y's) and data-tip (html).
(function () {
  document.addEventListener('mouseover', function (e) {
    const cell = e.target.closest && e.target.closest('.chart .hits > div');
    if (!cell) return;
    const chart = cell.closest('.chart');
    const x = parseFloat(cell.dataset.x);
    const xhair = chart.querySelector('.xhair');
    if (xhair) {
      xhair.style.opacity = 1;
      xhair.querySelector('line').setAttribute('x1', x);
      xhair.querySelector('line').setAttribute('x2', x);
      const ys = (cell.dataset.ys || '').split(';');
      xhair.querySelectorAll('circle').forEach(function (c, i) { c.setAttribute('cx', x); c.setAttribute('cy', ys[i]); });
    }
    const tip = chart.querySelector('.tip');
    if (tip) {
      tip.innerHTML = cell.dataset.tip;
      tip.style.left = Math.min(86, Math.max(14, x / 720 * 100)) + '%';
      tip.style.opacity = 1;
    }
  });
  document.addEventListener('mouseout', function (e) {
    const hits = e.target.closest && e.target.closest('.chart .hits');
    if (!hits || hits.contains(e.relatedTarget)) return;
    const chart = hits.closest('.chart');
    const xhair = chart.querySelector('.xhair'); if (xhair) xhair.style.opacity = 0;
    const tip = chart.querySelector('.tip'); if (tip) tip.style.opacity = 0;
  });
})();
