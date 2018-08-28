# Getting Makefile directory absolute path, see https://stackoverflow.com/questions/18136918/how-to-get-current-relative-directory-of-your-makefile/23324703#23324703
TESTS_DIR := $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

all: sqlite mysql postgres sqlserver

sqlite:
	rm -f DbSchemaValidator.db
	sqlite3 DbSchemaValidator.db < SQL.Common/CreateTables.sql

mysql:
	docker run --name DbSchemaValidator.Tests.MySQL -e MYSQL_ROOT_PASSWORD=docker -e MYSQL_DATABASE=DbSchemaValidator -e MYSQL_ROOT_HOST=% --volume "${TESTS_DIR}/SQL.MySQL:/docker-entrypoint-initdb.d" --publish 3306:3306 --detach mysql/mysql-server:5.7

postgres:
	docker run --name DbSchemaValidator.Tests.Npgsql -e POSTGRES_PASSWORD=docker -e POSTGRES_DB=DbSchemaValidator --volume "${TESTS_DIR}/SQL.Common:/docker-entrypoint-initdb.d" --publish 5432:5432 --detach postgres:10.5-alpine

sqlserver:
	docker run --name DbSchemaValidator.Tests.SqlServer -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=SqlServer-doc4er --volume "${TESTS_DIR}/SQL.SqlServer:/docker-entrypoint-initdb.d" --publish 1433:1433 --detach genschsa/mssql-server-linux

reset:
	docker rm -f DbSchemaValidator.Tests.MySQL
	docker rm -f DbSchemaValidator.Tests.Npgsql
	docker rm -f DbSchemaValidator.Tests.SqlServer
