import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Em desenvolvimento o Vite encaminha /api para a API, do mesmo jeito que
      // o nginx faz em produção. Com isso o cliente chama sempre a mesma
      // origem, e não há configuração de CORS para manter nos dois ambientes.
      '/api': {
        target: process.env.API_URL ?? 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
})
