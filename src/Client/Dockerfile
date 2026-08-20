# => Build container
# Node 16 - последний LTS эпохи этого фронтенда (webpack 3, TypeScript 3.7).
# Плавающий тег node:alpine сегодня даёт Node 26, где, во-первых, больше не
# поставляется yarn (RUN yarn -> "yarn: not found"), а во-вторых, OpenSSL 3
# убрал хеш md4, на который опирается webpack 3.
FROM node:16-alpine AS builder

ARG SECURITY_API_URL
ARG SALES_API_URL
ARG PRODUCTION_API_URL
ARG PUSH_HUBS_URL

WORKDIR /app
COPY package.json .
COPY yarn.lock .
RUN yarn
COPY . .
RUN yarn build --env.NODE_ENV=production --env.REACT_APP_SECURITY_API_URL=$SECURITY_API_URL --env.REACT_APP_SALES_API_URL=$SALES_API_URL --env.REACT_APP_PRODUCTION_API_URL=$PRODUCTION_API_URL --env.REACT_APP_NOTIFICATION_PUSH_HUBS_URL=$PUSH_HUBS_URL

# => Run container
FROM nginx

# Nginx config
RUN rm -rf /etc/nginx/conf.d
COPY conf /etc/nginx

# Static build
COPY --from=builder /app/dist /usr/share/nginx/html/

CMD ["nginx", "-g", "daemon off;"]