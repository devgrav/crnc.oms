# => Build container
FROM node:alpine as builder

ARG SECURITY_API_URL
ARG SALES_API_URL

WORKDIR /app
COPY package.json .
COPY yarn.lock .
RUN yarn
COPY . .
RUN yarn build --env NODE_ENV=production REACT_APP_SECURITY_API_URL=$SECURITY_API_URL REACT_APP_SALES_API_URL=$SALES_API_URL

# => Run container
FROM nginx:1.15.2-alpine

# Nginx config
RUN rm -rf /etc/nginx/conf.d
COPY conf /etc/nginx

# Static build
COPY --from=builder /app/build /usr/share/nginx/html/

# Default port exposure
EXPOSE 80

# Copy .env file and shell script to container
WORKDIR /usr/share/nginx/html

# Start Nginx server
CMD ["nginx -g \"daemon off;\""]