// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import dotenv from 'dotenv';
import tailwindcss from '@tailwindcss/vite'

// Load environment variables
dotenv.config();

export default defineConfig({
  plugins: [tailwindcss(),react()],
  server: {
    port: Number(process.env.PORT) || 4001,
    proxy: {
      '/api': {
        target: process.env.services__apigateway__https__0 || process.env.services__apigateway__http__0,
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
        secure: false,
      },
    },
  },
  build: {
    outDir: 'dist',
    rollupOptions: {
      output: {
        entryFileNames: 'bundle.js',
      },
    },
  },
});
