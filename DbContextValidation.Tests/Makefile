# Getting Makefile directory absolute path, see https://stackoverflow.com/questions/18136918/how-to-get-current-relative-directory-of-your-makefile/23324703#23324703
TESTS_DIR := $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

setup: setup_sqlite setup_mysql setup_postgres setup_sqlserver

teardown: teardown_mysql teardown_postgres teardown_sqlserver

setup_sqlite:
	rm -f DbContextValidation.db
	sqlite3 DbContextValidation.db < SQL.Common/CreateTables.sql

setup_mysql: teardown_mysql
	docker run --name DbContextValidation.Tests.MySQL -e MYSQL_ROOT_PASSWORD=docker -e MYSQL_DATABASE=DbContextValidation -e MYSQL_ROOT_HOST=% --volume "${TESTS_DIR}/SQL.MySQL:/docker-entrypoint-initdb.d:ro" --publish 3306:3306 --detach mysql/mysql-server:5.7

setup_postgres: teardown_postgres
	docker run --name DbContextValidation.Tests.Npgsql -e POSTGRES_PASSWORD=docker -e POSTGRES_DB=DbContextValidation --volume "${TESTS_DIR}/SQL.Common:/docker-entrypoint-initdb.d:ro" --publish 5432:5432 --detach postgres:10.5-alpine

setup_sqlserver: teardown_sqlserver
	docker run --name DbContextValidation.Tests.SqlServer -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=SqlServer-doc4er --volume "${TESTS_DIR}/SQL.SqlServer:/docker-entrypoint-initdb.d:ro" --publish 1433:1433 --detach genschsa/mssql-server-linux

teardown_mysql:
	docker rm -f DbContextValidation.Tests.MySQL || true

teardown_postgres:	
	docker rm -f DbContextValidation.Tests.Npgsql || true

teardown_sqlserver:
	docker rm -f DbContextValidation.Tests.SqlServer || true
