# => Build container
# Мажор ноды прибит намеренно: плавающий тег node:alpine ломает сборку в момент,
# когда апстрим переезжает на следующий мажор. Vite 8 требует ^20.19 || >=22.12.
FROM node:22-alpine AS builder

# Коммит сборки: версия приложения берётся из package.json, а хеш знает только
# тот, кто собирает. Не передали - в UI останется "dev", и это честно.
ARG GIT_COMMIT=""
ENV GITHUB_SHA=$GIT_COMMIT

WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# => Run container
FROM nginx

# Nginx config
RUN rm -rf /etc/nginx/conf.d
COPY conf /etc/nginx

# Static build
COPY --from=builder /app/dist /usr/share/nginx/html/

CMD ["nginx", "-g", "daemon off;"]
