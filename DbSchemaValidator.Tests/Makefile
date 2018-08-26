# Getting Makefile directory absolute path, see https://stackoverflow.com/questions/18136918/how-to-get-current-relative-directory-of-your-makefile/23324703#23324703
TESTS_DIR := $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

all: sqlite postgres mysql

sqlite:
	rm -f DbSchemaValidator.db
	sqlite3 DbSchemaValidator.db < SQL.Common/CreateTables.sql

postgres:
	docker run --name DbSchemaValidator.PostgreSQL -e POSTGRES_PASSWORD=docker -e POSTGRES_DB=DbSchemaValidator --volume "${TESTS_DIR}/SQL.Common:/docker-entrypoint-initdb.d" --publish 5432:5432 --detach postgres:10.5-alpine

mysql:
	docker run --name DbSchemaValidator.MySQL -e MYSQL_ROOT_PASSWORD=docker -e MYSQL_DATABASE=DbSchemaValidator -e MYSQL_ROOT_HOST=% --volume "${TESTS_DIR}/SQL.MySQL:/docker-entrypoint-initdb.d" --publish 3306:3306 --detach mysql/mysql-server:5.7

reset:
	docker stop DbSchemaValidator.MySQL
	docker rm DbSchemaValidator.MySQL
	docker stop DbSchemaValidator.PostgreSQL
	docker rm DbSchemaValidator.PostgreSQL
