/**
 * Everything that identifies the bakery lives here. Replace the placeholders and the whole site
 * (navigation, footer, order messages, contact page) updates. Also update the JSON-LD block in index.html
 * and the canonical / og:image URLs there once the real domain is known.
 */
export const brand = {
  name: 'Talukder Foods',
  short: 'Talukder',
  tagline: 'Made slowly. Loved deeply.',
  statement: 'A small kitchen, a slow clock and real butter. We bake cakes, pastries and breads by hand, every morning, in small batches.',
  established: 'Est. 2021',
  currency: '৳',
  location: 'Dhaka, Bangladesh',
  address: ['House 00, Road 00', 'Your Area, Dhaka 1200'],
  phone: '+880 1700-000000',
  whatsapp: '8801700000000', // digits only, with country code. Used for "Order now" messages.
  email: 'hello@example.com',
  mapsUrl: 'https://maps.google.com/?q=Dhaka',
  facebook: 'https://www.facebook.com/share/1Bw2qaF2P9/',
  instagram: 'https://www.instagram.com/talukderfoods',
  instagramHandle: '@talukderfoods',
  hours: [
    { days: 'Saturday to Thursday', time: '9:00 AM to 9:00 PM' },
    { days: 'Friday', time: '3:00 PM to 10:00 PM' },
  ],
  leadTime: 'Celebration cakes need 3 days notice. Everything else is baked fresh each morning.',
} as const;

export type Brand = typeof brand;
