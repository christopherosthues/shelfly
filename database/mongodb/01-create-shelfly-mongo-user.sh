#!/usr/bin/env bash
set -euo pipefail

: "${MONGO_HOST:?MONGO_HOST is required}"
: "${MONGODB_ROOT_USER:?MONGODB_ROOT_USER is required}"
: "${MONGODB_SHELFLY_DATABASE:?MONGODB_SHELFLY_DATABASE is required}"
: "${MONGODB_SHELFLY_CONFIG_USER:?MONGODB_SHELFLY_CONFIG_USER is required}"

MONGO_INITDB_ROOT_PASSWORD=$(cat /run/secrets/mongodb_root_password)
MONGODB_SHELFLY_CONFIG_PASSWORD=$(cat /run/secrets/mongodb_shelfly_config_password)

echo "Creating MongoDB application user..."

mongosh \
    --host "$MONGO_HOST" \
    --username "$MONGODB_ROOT_USER" \
    --password "$MONGO_INITDB_ROOT_PASSWORD" \
    --authenticationDatabase admin \
    --quiet \
    --eval '
        const databaseName = process.env.MONGODB_SHELFLY_DATABASE;
        const username = process.env.MONGODB_SHELFLY_CONFIG_USER;
        const password = process.env.MONGODB_SHELFLY_CONFIG_PASSWORD;

        const database = db.getSiblingDB(databaseName);
        const existingUser = database.getUser(username);

        if (existingUser) {
            print(`User ${username} already exists`);
        } else {
            database.createUser({
                user: username,
                pwd: password,
                roles: [
                    {
                        role: "readWrite",
                        db: databaseName
                    }
                ]
            });

            print(`Created user ${username}`);
        }
    '

echo "MongoDB initialization complete."