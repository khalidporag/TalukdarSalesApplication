import { brand } from '@/data/brand';
import { money } from './format';

export interface OrderLine {
  slug: string;
  name: string;
  variant: string;
  price: number;
  qty: number;
}

export interface OrderDetails {
  name: string;
  phone: string;
  when: string;
  note: string;
}

export interface Order {
  lines: OrderLine[];
  details: OrderDetails;
}

export const orderTotal = (lines: OrderLine[]) => lines.reduce((a, l) => a + l.price * l.qty, 0);

export function buildOrderMessage(order: Order): string {
  const rows = order.lines.map((l) => `• ${l.qty} x ${l.name} (${l.variant}) ${money(l.price * l.qty)}`);
  return [
    `Hello ${brand.name}, I would like to place an order:`,
    '',
    ...rows,
    '',
    `Total: ${money(orderTotal(order.lines))}`,
    order.details.when ? `Needed on: ${order.details.when}` : '',
    order.details.note ? `Note: ${order.details.note}` : '',
    '',
    `Name: ${order.details.name || '-'}`,
    `Phone: ${order.details.phone || '-'}`,
  ].filter((l, i, a) => l !== '' || a[i - 1] !== '').join('\n');
}

export const whatsappLink = (message: string) => `https://wa.me/${brand.whatsapp}?text=${encodeURIComponent(message)}`;

/**
 * The seam for a real backend. Today an order is handed to WhatsApp; to post it to an API instead,
 * replace the body of this function (the UI only awaits it).
 */
export async function submitOrder(order: Order): Promise<{ channel: 'whatsapp'; url: string }> {
  return { channel: 'whatsapp', url: whatsappLink(buildOrderMessage(order)) };
}
