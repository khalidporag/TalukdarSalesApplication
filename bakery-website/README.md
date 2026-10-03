# Talukder Foods: bakery website

A static, premium, editorial website for a boutique bakery. No backend. Built with React, TypeScript, Vite, Tailwind CSS, Framer Motion and Lucide icons.

**The logo is the real Talukder Foods mark; the rest of the brand copy is placeholder.** The reference Facebook page could not be read while building (it needs a login and was not reachable), so the identity, products, prices and copy are invented and clearly marked for replacement. Everything you need to change lives in `src/data/`.

## Run it

    cd bakery-website
    npm install
    npm run dev          # http://localhost:5173
    npm run build        # typecheck + production build in dist/ (also writes 404.html, sitemap.xml, robots.txt)
    npm run preview      # serve dist/

Deploy `dist/` to any static host (Netlify, Cloudflare Pages, GitHub Pages, S3, IIS). For a sub-path such as GitHub Pages: `BASE_PATH=/repo-name/ SITE_URL=https://you.github.io/repo-name npm run build`. `404.html` is a copy of `index.html`, which makes deep links like `/menu` work on hosts that support a 404 fallback.

## Make it yours (in this order)

1. **`src/data/brand.ts`**: name, tagline, statement, address, phone, WhatsApp number (digits only), hours, social links, currency. The navigation, footer, contact section and order messages all read from it.
2. **`index.html`**: update the title, description, canonical URL, `og:image` URL and the JSON-LD `Bakery` block (name, address, phone, hours, `sameAs`). `public/og.jpg` is the share image (1200 x 630); replace it with a real photo.
3. **`src/lib/hours.ts`**: the "Open now" label. Keep it in step with the hours in `brand.ts`.
4. **`src/data/products.ts`**: your menu. One object per product (name, category, description, sizes and prices, ingredients, whether it is a "signature"). Slugs become URLs (`/product/<slug>`) and appear in `sitemap.xml` after the next build.
5. **`src/data/testimonials.ts`**, **`gallery.ts`**, **`social.ts`**: copy and captions.

## Photos

The site ships with original illustrated artwork so it looks finished before you have photography. Real photos replace it automatically:

- Put files in `src/assets/photos/` (jpg, png, webp or avif).
- Name a file after a product slug (`butter-croissant.jpg`) and it is used on every card and page for that product.
- Other names: `hero`, `hero-detail`, `story`, `story-oven`, `basque-cheesecake-feature` (the featured section), `kitchen-mix`, `kitchen-bake`, `kitchen-decorate`, `kitchen-serve`, `gallery-1` to `gallery-9`, `social-1` to `social-6`, `custom-birthday`, `custom-wedding`, `custom-anniversary`, `custom-corporate`, `custom-custom`, `custom-enquiry`.
- Photos are fingerprinted, lazy-loaded and decoded asynchronously. For best results export at about 1600 px on the long edge as WebP or AVIF.

## How ordering works (no backend)

Visitors choose sizes, add items to an order basket (kept in the browser) and press **Send order on WhatsApp**, which opens a pre-written message to your number. The custom cake enquiry form does the same. Nothing is stored on a server.

To connect a real backend later, change one function: `submitOrder` in `src/lib/orders.ts` (and `getProducts` / `getSocialPosts` if you want the menu or feed to come from an API). The components only depend on those functions and on the shapes in `src/data/`.

## Structure

    src/
      art/           illustrated product art (SVG), tones and the <Art> component
      components/    Navbar, Hero, BrandStory, SignatureProducts, FeaturedProduct, Menu, BakeryExperience,
                     CustomCake, Gallery, Testimonials, SocialSection, ContactSection, Footer, order drawer ...
      pages/         Home, MenuPage, ProductDetails, About, Contact, NotFound
      data/          brand, categories, products, testimonials, gallery, social
      lib/           orders (WhatsApp + submit seam), seo (per-page meta), hours, photos, format
    scripts/postbuild.mjs   404 fallback, sitemap, robots
    dev/                    contact sheet of all artwork and the share-image generator (dev only, not built)

## Design notes

- Palette: ivory and cream foundation, espresso and cocoa for depth, one burnished caramel accent. Cormorant Garamond for display, Manrope for text, both self-hosted.
- Motion is slow and quiet (one easing curve, 0.9 to 1.4 s). The system `prefers-reduced-motion` setting turns it off. The custom cursor only appears with a mouse.
- Below-the-fold sections are code-split and mounted when they approach the viewport. The first load is about 100 KB of JavaScript (gzipped) plus 40 KB for Framer Motion.
- SEO: per-page title, description and canonical (`src/lib/seo.ts`), Open Graph tags, `Bakery`, `Menu` and `Product` structured data, sitemap and robots.
