#!/usr/bin/env bash
set -euo pipefail

: "${MONGO_HOST:?MONGO_HOST is required}"
: "${MONGO_INITDB_ROOT_USERNAME:?MONGO_INITDB_ROOT_USERNAME is required}"
: "${MONGO_INITDB_ROOT_PASSWORD:?MONGO_INITDB_ROOT_PASSWORD is required}"
: "${MONGODB_SHELFLY_DATABASE:?MONGODB_SHELFLY_DATABASE is required}"
: "${MONGODB_SHELFLY_CONFIG_USER:?MONGODB_SHELFLY_CONFIG_USER is required}"
: "${MONGODB_SHELFLY_CONFIG_PASSWORD:?MONGODB_SHELFLY_CONFIG_PASSWORD is required}"

echo "Creating MongoDB application user..."

mongosh \
    --host "$MONGO_HOST" \
    --username "$MONGO_INITDB_ROOT_USERNAME" \
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