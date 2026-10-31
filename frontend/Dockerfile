# Imagem do cliente. Multiestágio: o Node compila, o nginx serve os estáticos.

# ---------- compilação ----------
FROM node:22-alpine AS compilacao
WORKDIR /origem

# package.json e lock antes do código, para reaproveitar a camada de instalação.
COPY package.json package-lock.json ./
RUN npm ci --no-fund --no-audit

COPY . .
RUN npm run build

# ---------- execução ----------
FROM nginx:1.27-alpine AS execucao

COPY --from=compilacao /origem/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
