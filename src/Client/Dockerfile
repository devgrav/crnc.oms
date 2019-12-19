# => Build container
FROM node:alpine as builder

ARG SECURITY_API_URL
ARG SALES_API_URL

WORKDIR /app
COPY package.json .
COPY yarn.lock .
RUN yarn
COPY . .
RUN yarn build --env.NODE_ENV=production --env.REACT_APP_SECURITY_API_URL=$SECURITY_API_URL --env.REACT_APP_SALES_API_URL=$SALES_API_URL

# => Run container
FROM nginx

# Nginx config
RUN rm -rf /etc/nginx/conf.d
COPY conf /etc/nginx

# Static build
COPY --from=builder /app/dist /usr/share/nginx/html/

CMD ["nginx", "-g", "daemon off;"]