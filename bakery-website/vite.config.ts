import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Deploy under a sub-path (e.g. GitHub Pages) with:  BASE_PATH=/repo-name/ npm run build
export default defineConfig({
  base: process.env.BASE_PATH ?? '/',
  plugins: [react()],
  resolve: { alias: { "@": new URL("./src", import.meta.url).pathname } },
  build: {
    target: 'es2020',
    chunkSizeWarningLimit: 700,
  },
});
