-- Idempotent database creation script for Shelfly (Execution Order: 01)
-- Mount point: /docker-entrypoint-initdb.d/01-create-shelfly-db.sql
-- Environment variables used: SHELFLY_DB (required)
-- Dependencies: None — runs first before user creation

\set db `echo ${SHELFLY_DB}`

-- Create the database if it does not already exist
SELECT 'CREATE DATABASE :' || :db;
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = :db)
\gexec
