FROM caddy:2-builder AS builder
RUN xcaddy build --with github.com/caddyserver/replace-response

FROM caddy:2 as final
ARG VERSION
ENV VERSION=$VERSION
RUN chmod +rw /config/caddy
COPY Caddyfile /etc/caddy/Caddyfile
COPY html/ /site/
EXPOSE 2016
