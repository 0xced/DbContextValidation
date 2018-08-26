# Getting Makefile directory absolute path, see https://stackoverflow.com/questions/18136918/how-to-get-current-relative-directory-of-your-makefile/23324703#23324703
TESTS_DIR := $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

all: sqlite postgres

sqlite:
	rm -f DbSchemaValidator.db
	sqlite3 DbSchemaValidator.db < SQL/CreateTables.sql

postgres:
	docker run --name DbSchemaValidator.PostgreSQL -e POSTGRES_PASSWORD=docker -e POSTGRES_DB=DbSchemaValidator --volume "${TESTS_DIR}/SQL:/docker-entrypoint-initdb.d" --publish 5432:5432 --detach postgres:10.5-alpine
