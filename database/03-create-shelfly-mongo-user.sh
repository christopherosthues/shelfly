#!/bin/bash
# Idempotent MongoDB initialization script for Shelfly configuration user (Execution Order: after root auth)
# Mount point: /docker-entrypoint-initdb.d/create-shelfly-config-user.sh
# Environment variables used: MONGODB_INITDB_ROOT_USERNAME, MONGODB_INITDB_ROOT_PASSWORD, MONGODB_SHELFLY_CONFIG_USER (default: shelfly_config_user), MONGODB_SHELFLY_CONFIG_PASSWORD
# Dependencies: Requires MongoDB container to be running and root authentication established

set -e

# Resolve username from environment variable with fallback default
SHELFLY_MONGO_USER="${MONGODB_SHELFLY_CONFIG_USER:-shelfly_config_user}"

# Wait for MongoDB to be ready
until mongosh --username "$MONGODB_INITDB_ROOT_USERNAME" --password "$MONGODB_INITDB_ROOT_PASSWORD" --eval "db.runCommand({ ping: 1 }).ok" localhost:27017/admin > /dev/null 2>&1; do
    echo "Waiting for MongoDB to be ready..."
    sleep 2
done

echo "MongoDB is ready — creating Shelfly configuration user ($SHELFLY_MONGO_USER)"

# Create dedicated user with readWrite role on shelfly-config database only
mongosh --username "$MONGODB_INITDB_ROOT_USERNAME" --password "$MONGODB_INITDB_ROOT_PASSWORD" localhost:27017/shelfly-config <<EOF
const existingUsers = db.getUsers({ roles: [{ role: 'readWrite', db: 'shelfly-config' }] });

if (existingUsers.length === 0) {
    db.createUser({
        user: '$SHELFLY_MONGO_USER',
        pwd: '$MONGODB_SHELFLY_CONFIG_PASSWORD',
        roles: [
            { role: 'readWrite', db: 'shelfly-config' }
        ]
    });
    print('Created $SHELFLY_MONGO_USER with readWrite access to shelfly-config');
} else {
    print('User $SHELFLY_MONGO_USER already exists — skipping creation');
}
EOF

echo "Shelfly configuration user setup complete"
