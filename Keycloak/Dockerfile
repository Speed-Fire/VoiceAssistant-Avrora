FROM debian:latest as builder
RUN apt-get update && apt-get install -y gettext && apt-get clean

FROM quay.io/keycloak/keycloak:25.0

ARG AUTH_CLIENT_ADMIN_ID
ARG AUTH_CLIENT_ADMIN_SECRET
ENV AUTH_CLIENT_ADMIN_ID=${AUTH_CLIENT_ADMIN_ID}
ENV AUTH_CLIENT_ADMIN_SECRET=${AUTH_CLIENT_ADMIN_SECRET}

USER root
COPY --from=builder /usr/bin/envsubst /usr/bin/envsubst



##RUN dnf install -y gettext && dnf clean all

COPY ["Keycloak/realms/*", "/app/Keycloak/"]
COPY ["Keycloak/prepare-realms.sh", "/app/"]

RUN chmod +x /app/prepare-realms.sh

RUN /app/prepare-realms.sh

USER keycloak