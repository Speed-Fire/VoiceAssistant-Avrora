#!/bin/bash

echo "Starting script..."

mkdir -p /opt/keycloak/data/import

for file in /app/Keycloak/*.template.json; do
	echo "$file"
	filename=$(basename "$file")
	name=${filename%.template.json}
	envsubst < "$file" > /opt/keycloak/data/import/${name}.json
	cat /opt/keycloak/data/import/${name}.json
done

echo $(ls -a /opt/keycloak/data/import/)

echo "Script completed."