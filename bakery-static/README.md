# Talukder Foods: static website

Plain HTML, CSS and JavaScript. No build step, no backend, no dependencies.

Open `index.html` in a browser, or serve the folder:

    python3 -m http.server 8000

## Edit

- **Products, categories, WhatsApp number:** the top of `app.js` (`WHATSAPP`, `PRODUCTS`). Image files live in `assets/img/`.
- **Prices, address, hours, phone, story:** the `[PLACEHOLDER]` text in `index.html`.
- **Colors:** the CSS variables at the top of `styles.css` (taken from the Talukder Foods logo).

Orders and custom-cake enquiries open WhatsApp with the message pre-filled. The basket is kept in the browser's localStorage.

Deploy by uploading the folder to any static host (GitHub Pages, Netlify, Cloudflare Pages).
