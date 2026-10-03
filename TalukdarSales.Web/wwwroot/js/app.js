// Small glue between htmx server events and Bootstrap widgets.
(function () {
  const modalEl = document.getElementById('app-modal');
  const modal = () => bootstrap.Modal.getOrCreateInstance(modalEl);

  // Server sends HX-Trigger: {"toast": {"message": "...", "success": true}}
  document.body.addEventListener('toast', function (e) {
    const d = e.detail || {};
    const el = document.createElement('div');
    el.className = 'toast align-items-center border-0 text-bg-' + (d.success === false ? 'danger' : 'success');
    el.setAttribute('role', 'alert');
    el.innerHTML = '<div class="d-flex"><div class="toast-body"></div>' +
      '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
    el.querySelector('.toast-body').textContent = d.message || '';
    document.getElementById('toasts').appendChild(el);
    const t = new bootstrap.Toast(el, { delay: 4000 });
    el.addEventListener('hidden.bs.toast', () => el.remove());
    t.show();
  });

  document.body.addEventListener('closeModal', () => modal().hide());

  // Show the modal whenever a fragment is swapped into it.
  document.body.addEventListener('htmx:afterSwap', function (e) {
    if (e.detail.target && e.detail.target.id === 'modal-content') modal().show();
  });

  // Failed requests: show a toast instead of silently doing nothing.
  document.body.addEventListener('htmx:responseError', function (e) {
    if (e.detail.xhr.status === 401) return; // redirect handled by HX-Redirect
    document.body.dispatchEvent(new CustomEvent('toast', { detail: { message: 'Something went wrong (' + e.detail.xhr.status + ').', success: false } }));
  });

  // Confirmation helper: <button data-confirm="Are you sure?" ...>
  document.body.addEventListener('htmx:confirm', function (e) {
    const msg = e.detail.elt.getAttribute('data-confirm');
    if (!msg) return;
    e.preventDefault();
    if (window.confirm(msg)) e.detail.issueRequest(true);
  });
})();
