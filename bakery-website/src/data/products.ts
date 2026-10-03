import type { CategoryId } from './categories';
import type { ArtKind, Tone } from '@/art/Art';

export interface Variant {
  label: string;
  price: number;
}

export interface Product {
  slug: string;
  name: string;
  category: CategoryId;
  /** One line for cards. */
  description: string;
  /** A few sentences for the detail view. */
  story: string;
  variants: Variant[];
  ingredients: string[];
  /** Appears in "Our Signatures". */
  signature?: boolean;
  /** Small label on the card, e.g. "Baked daily". */
  note?: string;
  /** Illustration used until a real photo is added (see README, "Photos"). */
  art: ArtKind;
  tone: Tone;
}

/**
 * Products are plain data so they can later come from an API: replace this array with a fetch in
 * src/data/api.ts (getProducts) and nothing in the components changes.
 */
export const products: Product[] = [
  {
    slug: 'butter-croissant',
    name: 'Butter Croissant',
    category: 'pastries',
    description: 'Seventy-two hours of lamination. Shatter-crisp outside, honeycomb within.',
    story: 'Our dough rests for three days and is folded by hand three times. The result is a croissant that crackles when you tear it and smells like the best part of a bakery.',
    variants: [{ label: 'Single', price: 220 }, { label: 'Box of 4', price: 820 }],
    ingredients: ['French-style butter', 'Unbleached flour', 'Sea salt', 'Fresh milk'],
    signature: true,
    note: 'Baked daily',
    art: 'croissant',
    tone: 'honey',
  },
  {
    slug: 'basque-cheesecake',
    name: 'Burnt Basque Cheesecake',
    category: 'cakes',
    description: 'Deeply caramelised top, barely-set custard centre, no base to get in the way.',
    story: 'Baked hot until the surface turns the colour of dark amber, then left to settle. Creamy, a little smoky and impossible to share politely.',
    variants: [{ label: 'Slice', price: 420 }, { label: '6 inch', price: 2400 }, { label: '8 inch', price: 3400 }],
    ingredients: ['Cream cheese', 'Cream', 'Free-range eggs', 'Vanilla bean'],
    signature: true,
    note: 'Most loved',
    art: 'cheesecake',
    tone: 'caramel',
  },
  {
    slug: 'dark-chocolate-cake',
    name: 'Dark Chocolate Ganache Cake',
    category: 'cakes',
    description: 'Four layers of cocoa sponge, a glossy 70% ganache, and a pinch of sea salt.',
    story: 'A moist sponge made with melted dark chocolate and strong coffee, sandwiched with whipped ganache and finished in a mirror-smooth glaze.',
    variants: [{ label: 'Slice', price: 460 }, { label: '1 lb', price: 1900 }, { label: '2 lb', price: 3600 }],
    ingredients: ['70% dark chocolate', 'Cocoa', 'Espresso', 'Cultured butter'],
    signature: true,
    art: 'chocolate-cake',
    tone: 'cocoa',
  },
  {
    slug: 'pistachio-rose-cake',
    name: 'Pistachio & Rose Cake',
    category: 'cakes',
    description: 'Soft pistachio sponge, rose cream and a scatter of crushed nuts and petals.',
    story: 'Fragrant rather than sweet. We grind pistachios fresh for the sponge and perfume the cream with a little rose water.',
    variants: [{ label: 'Slice', price: 480 }, { label: '1 lb', price: 2100 }, { label: '2 lb', price: 3900 }],
    ingredients: ['Pistachio', 'Rose water', 'Mascarpone', 'Dried rose petals'],
    signature: true,
    art: 'tiered',
    tone: 'sage',
  },
  {
    slug: 'seasonal-fruit-tart',
    name: 'Seasonal Fruit Tart',
    category: 'pastries',
    description: 'Buttery sablé shell, vanilla crème pâtissière, fruit picked that morning.',
    story: 'A thin, crisp shell filled with a custard flecked with real vanilla and topped with whatever is best this week.',
    variants: [{ label: 'Individual', price: 380 }, { label: '8 inch', price: 2600 }],
    ingredients: ['Sablé pastry', 'Vanilla custard', 'Seasonal fruit', 'Apricot glaze'],
    signature: true,
    art: 'tart',
    tone: 'blush',
  },
  {
    slug: 'chocolate-eclair',
    name: 'Chocolate Éclair',
    category: 'pastries',
    description: 'Crisp choux, silky chocolate crème, a mirror of dark glaze.',
    story: 'Choux baked until properly dry and crisp, filled to order-day freshness with a cream we cook slowly on the stove.',
    variants: [{ label: 'Single', price: 320 }, { label: 'Box of 4', price: 1200 }],
    ingredients: ['Choux pastry', 'Dark chocolate', 'Cream', 'Vanilla'],
    art: 'eclair',
    tone: 'cocoa',
  },
  {
    slug: 'brown-butter-cookie',
    name: 'Brown Butter Chocolate Chunk Cookie',
    category: 'cookies',
    description: 'Nutty brown butter, chopped dark chocolate, flaky salt. Crisp edge, soft middle.',
    story: 'The dough is chilled for two days so the flavour deepens. Each cookie is weighed, shaped by hand and baked until the edges just turn gold.',
    variants: [{ label: 'Single', price: 240 }, { label: 'Box of 6', price: 1300 }],
    ingredients: ['Brown butter', 'Dark chocolate', 'Muscovado', 'Flaky salt'],
    signature: true,
    note: 'Baked daily',
    art: 'cookies',
    tone: 'honey',
  },
  {
    slug: 'oat-date-cookie',
    name: 'Oat & Date Cookie',
    category: 'cookies',
    description: 'Chewy rolled oats, soft Medjool dates and a little cinnamon.',
    story: 'A gentler cookie for people who say they do not like sweet things. Chewy, spiced and quietly addictive.',
    variants: [{ label: 'Single', price: 200 }, { label: 'Box of 6', price: 1100 }],
    ingredients: ['Rolled oats', 'Medjool dates', 'Cinnamon', 'Brown sugar'],
    art: 'cookies',
    tone: 'sage',
  },
  {
    slug: 'country-sourdough',
    name: 'Country Sourdough',
    category: 'breads',
    description: 'Naturally leavened over 36 hours. Blistered crust, open crumb, a gentle tang.',
    story: 'Made from a starter we have fed daily since we opened. Baked in a steam oven until the crust sings when it cools.',
    variants: [{ label: 'Loaf', price: 520 }, { label: 'Half loaf', price: 290 }],
    ingredients: ['Wheat flour', 'Wholemeal', 'Water', 'Salt', 'Wild starter'],
    note: 'Out of the oven at 11 AM',
    art: 'loaf',
    tone: 'wheat',
  },
  {
    slug: 'brioche-loaf',
    name: 'Butter Brioche Loaf',
    category: 'breads',
    description: 'Tender, golden and rich with butter. Made for toast and for tearing.',
    story: 'An enriched dough with a long, cold rise. Slice it thick, toast it, and add nothing at all.',
    variants: [{ label: 'Loaf', price: 640 }],
    ingredients: ['Butter', 'Eggs', 'Milk', 'Flour'],
    art: 'loaf',
    tone: 'honey',
  },
  {
    slug: 'cinnamon-roll',
    name: 'Cardamom Cinnamon Roll',
    category: 'breads',
    description: 'Soft spiral, cardamom-scented sugar, cream cheese glaze.',
    story: 'A pillowy dough rolled thin with plenty of spiced butter, baked until the edges caramelise and glazed while warm.',
    variants: [{ label: 'Single', price: 340 }, { label: 'Box of 4', price: 1280 }],
    ingredients: ['Cardamom', 'Cinnamon', 'Cream cheese', 'Butter'],
    note: 'Weekend favourite',
    art: 'roll',
    tone: 'caramel',
  },
  {
    slug: 'tiramisu-jar',
    name: 'Classic Tiramisu',
    category: 'desserts',
    description: 'Espresso-soaked savoiardi, airy mascarpone, a dusting of cocoa.',
    story: 'Made the slow way: egg yolks whisked over heat, mascarpone folded in by hand, and a full night in the fridge to set.',
    variants: [{ label: 'Jar', price: 520 }, { label: 'Tray (serves 8)', price: 3800 }],
    ingredients: ['Mascarpone', 'Espresso', 'Savoiardi', 'Cocoa'],
    art: 'jar',
    tone: 'cocoa',
  },
  {
    slug: 'fudge-brownie',
    name: 'Sea Salt Fudge Brownie',
    category: 'desserts',
    description: 'Crackly top, dense fudgy centre, finished with flaky sea salt.',
    story: 'Made with melted dark chocolate rather than cocoa alone, which is why the middle stays dense and glossy.',
    variants: [{ label: 'Single', price: 300 }, { label: 'Box of 6', price: 1650 }],
    ingredients: ['Dark chocolate', 'Butter', 'Eggs', 'Sea salt'],
    art: 'brownie',
    tone: 'cocoa',
  },
  {
    slug: 'macaron-box',
    name: 'Macaron Gift Box',
    category: 'specials',
    description: 'Six delicate shells in rotating flavours: pistachio, raspberry, salted caramel.',
    story: 'Almond shells with a proper foot and a chewy middle, filled with ganaches and buttercreams we make in-house. Packed in a ribbon-tied box.',
    variants: [{ label: 'Box of 6', price: 1100 }, { label: 'Box of 12', price: 2100 }],
    ingredients: ['Almond flour', 'Egg whites', 'Seasonal fillings'],
    note: 'Gift ready',
    art: 'macarons',
    tone: 'blush',
  },
  {
    slug: 'celebration-cake',
    name: 'The Celebration Cake',
    category: 'specials',
    description: 'Our made-to-order showpiece. Tiered, hand-finished, designed around your day.',
    story: 'Every celebration cake begins with a conversation. Tell us the occasion, the people and the flavours you love, and we design and bake it from scratch.',
    variants: [{ label: '2 lb, 1 tier', price: 4200 }, { label: '4 lb, 2 tier', price: 8200 }, { label: 'Custom quote', price: 0 }],
    ingredients: ['Vanilla bean sponge', 'Swiss meringue buttercream', 'Fresh flowers', 'Your story'],
    note: 'Made to order',
    art: 'tiered',
    tone: 'ivory',
  },
];

export const startingPrice = (p: Product) => Math.min(...p.variants.filter((v) => v.price > 0).map((v) => v.price));
export const bySlug = (slug: string) => products.find((p) => p.slug === slug);
