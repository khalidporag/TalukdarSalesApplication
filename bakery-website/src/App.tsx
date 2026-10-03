import { AnimatePresence, MotionConfig, motion } from 'framer-motion';
import { lazy, Suspense } from 'react';
import { Route, Routes, useLocation } from 'react-router-dom';
import { BackToTop, ScrollProgress } from '@/components/ScrollUi';
import { CustomCursor } from '@/components/CustomCursor';
import { Loader } from '@/components/Loader';
import { Navbar } from '@/components/Navbar';
import { OrderDrawer } from '@/components/OrderDrawer';
import { OrderProvider } from '@/components/OrderContext';
import { ScrollManager } from '@/components/ScrollManager';
import { Footer } from '@/components/Footer';
import Home from '@/pages/Home';

const MenuPage = lazy(() => import('@/pages/MenuPage'));
const ProductDetails = lazy(() => import('@/pages/ProductDetails'));
const About = lazy(() => import('@/pages/About'));
const Contact = lazy(() => import('@/pages/Contact'));
const NotFound = lazy(() => import('@/pages/NotFound'));

export default function App() {
  const location = useLocation();
  return (
    <MotionConfig reducedMotion="user">
      <OrderProvider>
        <div className="grain">
          <a href="#main" className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-[110] focus:bg-espresso focus:px-4 focus:py-3 focus:text-ivory">Skip to content</a>
          <Loader />
          <ScrollProgress />
          <CustomCursor />
          <Navbar />
          <ScrollManager />
          <AnimatePresence mode="wait" initial={false}>
            <motion.main key={location.pathname} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} transition={{ duration: 0.45 }} id="main">
              <Suspense fallback={<div className="min-h-screen" />}>
                <Routes location={location}>
                  <Route path="/" element={<Home />} />
                  <Route path="/menu" element={<MenuPage />} />
                  <Route path="/product/:slug" element={<ProductDetails />} />
                  <Route path="/about" element={<About />} />
                  <Route path="/contact" element={<Contact />} />
                  <Route path="*" element={<NotFound />} />
                </Routes>
              </Suspense>
            </motion.main>
          </AnimatePresence>
          <Footer />
          <OrderDrawer />
          <BackToTop />
        </div>
      </OrderProvider>
    </MotionConfig>
  );
}
