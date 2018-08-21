all: sqlite postgres

sqlite:
	rm -f DbSchemaValidator.db
	sqlite3 DbSchemaValidator.db < CreateTables.sql

postgres:
	psql -q -c 'DROP DATABASE IF EXISTS "DbSchemaValidator"'
	psql -q -c 'CREATE DATABASE "DbSchemaValidator"'
	psql -q -d DbSchemaValidator -f CreateTables.sql
