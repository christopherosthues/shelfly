-- Idempotent user creation and permission grant script for Shelfly (Execution Order: 02)
-- Mount point: /docker-entrypoint-initdb.d/02-create-shelfly-user.sql
-- Environment variables used: SHELFLY_USER_PASSWORD (required), SHELFLY_DB (default: shelfly), SHELFLY_USER (default: shelfly_user)
-- Dependencies: Requires database to exist first (runs after 01-create-shelfly-db.sql)

-- Resolve username from environment variable with fallback default
SELECT COALESCE(NULLIF(getenv('SHELFLY_USER'), ''), 'shelfly_user') AS resolved_username \gset

-- Create the user if it does not already exist
SELECT 'CREATE USER :' || :resolved_username || ' WITH PASSWORD ''' || getenv('SHELFLY_USER_PASSWORD') || ''';'
WHERE NOT EXISTS (SELECT FROM pg_catalog.pg_user WHERE usename = :resolved_username)
\gexec

-- Grant read/write access to the shelfly database for the user
GRANT ALL ON DATABASE shelfly TO :resolved_username;

-- Connect to shelfly database and grant schema-level permissions
\c shelfly

-- Grant usage on public schema (allows reading/writing tables)
GRANT USAGE ON SCHEMA public TO :resolved_username;

-- Grant read/write/execute privileges on all objects in public schema
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO :resolved_username;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO :resolved_username;

-- Set default privileges for future tables/functions created by other users
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :resolved_username;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT EXECUTE ON FUNCTIONS TO :resolved_username;
