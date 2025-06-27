# 1. Build aşaması
FROM node:20-alpine AS builder
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# 2. Production aşaması (statik dosyaları sunmak için nginx)
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=builder /app/dist .
COPY --from=builder /app/public ./public
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]