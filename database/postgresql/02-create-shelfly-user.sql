-- Idempotent application user creation and permission grant script for Shelfly (Execution Order: 02)
-- Mount point: /docker-entrypoint-initdb.d/02-create-shelfly-user.sql
-- Environment variables used: SHELFLY_DB (required), SHELFLY_USER (required), SHELFLY_SCHEMA (required)
-- Secrets required: shelfly_user_password
-- Dependencies: Requires database and admin user to exist first (runs after 01b-create-shelfly-admin.sql)

\set schema `echo ${SHELFLY_SCHEMA}`
\set db `echo ${SHELFLY_DB}`
\set username `echo ${SHELFLY_USER}`

-- Create the application user if it does not already exist
SELECT 'CREATE USER :' || :username || ' WITH PASSWORD ''' || trim(pg_read_file('/run/secrets/shelfly_user_password')) || ''';'
WHERE NOT EXISTS (SELECT FROM pg_catalog.pg_user WHERE usename = :username)
\gexec

-- Grant read/write access to the database for the user
GRANT ALL ON DATABASE :db TO :username;

-- Connect to database and grant schema-level permissions
\c :db

-- Grant usage on configured schema (allows reading/writing tables)
GRANT USAGE ON SCHEMA :schema TO :username;

-- Grant read/write/execute privileges on all objects in configured schema
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA :schema TO :username;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA :schema TO :username;

-- Set default privileges for future tables/functions created by other users
ALTER DEFAULT PRIVILEGES IN SCHEMA :schema GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :username;
ALTER DEFAULT PRIVILEGES IN SCHEMA :schema GRANT EXECUTE ON FUNCTIONS TO :username;
