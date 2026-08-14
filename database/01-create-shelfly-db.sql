-- Idempotent database creation script for Shelfly (Execution Order: 01)
-- Mount point: /docker-entrypoint-initdb.d/01-create-shelfly-db.sql
-- Environment variables used: SHELFLY_DB (default: shelfly)
-- Dependencies: None — runs first before user creation

-- Create the shelfly database if it does not already exist
SELECT 'CREATE DATABASE shelfly;'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'shelfly')
\gexec
