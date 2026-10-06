# Talukder Foods Sales: live demo (no backend)

A clickable, static preview of the sales application. It uses the same stylesheet and layout as the real ASP.NET app, but all data is sample data generated in the browser and kept in `localStorage`. Nothing is sent anywhere.

Open `index.html` (or serve the folder with `python3 -m http.server`) and press **Sign in**.

## Try this flow
1. **New order**: pick a customer, tap + on a few products, Place order.
2. **Orders to invoice**: select orders, Create invoices.
3. **Invoices**: open one, enter an amount, Record payment.
4. **Dashboard / KPI report**: figures and charts update from what you just did.

5. **Production plan**: pick a day to see what to bake, totalled from that day's orders. Place a new order and it appears under today.
6. **Products / Customers / Users**: add a product (with an optional photo), a customer, or a user with a role. New products appear in New order straight away.

Use **Reset demo data** (top of any page) to start over.

Live at `/sales-demo/` on the GitHub Pages site (https://khalidporag.github.io/TalukdarSalesApplication/sales-demo/).

Not included in the demo: role permission editing, audit log, order window settings, returns and Excel export. Those exist only in the real app.
