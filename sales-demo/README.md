# Talukder Foods Sales: live demo (no backend)

A clickable, static preview of the sales application. It uses the same stylesheet and layout as the real ASP.NET app, but all data is sample data generated in the browser and kept in `localStorage`. Nothing is sent anywhere.

Open `index.html` (or serve the folder with `python3 -m http.server`) and press **Sign in**.

## Sign in as different people
Password for everyone is `demo1234`.

| Username | Role | Sees |
|---|---|---|
| `admin` | Administrator | everything |
| `tahmina` | Sales manager | everything except users, roles, order window, audit |
| `farhana` | Accountant | dashboard, reports, invoices, payments, discounts, returns, collections |
| `sabbir`, `nusrat`, `imran` | Salesperson | new orders, order list, invoices, production plan |

As admin you can also use **Admin, Users, Sign in as**. Menus and pages follow the role; a blocked page shows "Access denied".

## What is in the demo
- **Overview:** notices, dashboard with hover chart, KPI report, notifications (unread count, mark read).
- **Sell:** new order (customer search, credit-limit warning, order-window check), orders to invoice (edit, cancel, invoice one or many), invoices (status, date and search filters, paging, print, Excel export), give a discount, return items with a credit note, collect a payment, collect from a customer (clears oldest invoices first), collections.
- **Operate:** production plan for any day (print, Excel), products (add, edit, hide, photo), categories (add, rename, delete), customers (search, add, edit, delete, statement with date range, Excel).
- **Admin:** users (add, edit, reset password, disable, sign in as), roles and permissions, order window, audit log with filters.

## Try this flow
1. **New order**: pick a customer, tap + on a few products, Place order.
2. **Orders to invoice**: edit or cancel, or select orders and create invoices.
3. **Invoices**: open one, give a discount, return an item, record a payment.
4. **Audit log** and **Notifications** show every step. **Dashboard** and **Production plan** update too.

Use **Reset demo data** (top of any page) to start over.

## Notes
- "Excel" downloads open in Excel as CSV files.
- The order desk only takes orders inside its window (default 5 AM to 10 PM, local time). If it shows closed, an admin can change it under Order window.
- Not the real thing: there is no server, so nothing is shared between people and permissions are checked only in the browser.

Live at `/sales-demo/` on the GitHub Pages site (https://khalidporag.github.io/TalukdarSalesApplication/sales-demo/).
