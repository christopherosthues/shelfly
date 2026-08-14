-- Idempotent user creation and permission grant script for Shelfly (Execution Order: 02)
-- Mount point: /docker-entrypoint-initdb.d/02-create-shelfly-user.sql
-- Environment variables used: SHELFLY_USER_PASSWORD (required), SHELFLY_DB (default: shelfly)
-- Dependencies: Requires database to exist first (runs after 01-create-shelfly-db.sql)

-- Create the shelfly_user if it does not already exist
SELECT 'CREATE USER shelfly_user WITH PASSWORD ''' || getenv('SHELFLY_USER_PASSWORD') || ''';'
WHERE NOT EXISTS (SELECT FROM pg_catalog.pg_user WHERE usename = 'shelfly_user')
\gexec

-- Grant read/write access to the shelfly database for shelfly_user
GRANT ALL ON DATABASE shelfly TO shelfly_user;

-- Connect to shelfly database and grant schema-level permissions
\c shelfly

-- Grant usage on public schema (allows reading/writing tables)
GRANT USAGE ON SCHEMA public TO shelfly_user;

-- Grant read/write/execute privileges on all objects in public schema
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO shelfly_user;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO shelfly_user;

-- Set default privileges for future tables/functions created by other users
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO shelfly_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO shelfly_user;
