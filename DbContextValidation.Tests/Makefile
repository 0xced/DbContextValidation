# Getting Makefile directory absolute path, see https://stackoverflow.com/questions/18136918/how-to-get-current-relative-directory-of-your-makefile/23324703#23324703
TESTS_DIR := $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

test: test_ef6 test_efcore

test_ef6: build_ef6
	mono ~/.nuget/packages/xunit.runner.console/2.4.0/tools/net452/xunit.console.exe "${TESTS_DIR}"/../DbContextValidation.Tests.EF6.*/bin/Debug/*/*.Tests.EF6.*.dll -noshadow -html Report.EF6.html

test_efcore: build_efcore
	dotnet ~/.nuget/packages/xunit.runner.console/2.4.0/tools/netcoreapp2.0/xunit.console.dll "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.*/bin/Debug/*/osx-x64/publish/*.Tests.EFCore.*.dll -html Report.EFCore.html

build: build_ef6 build_efcore

build_ef6:
	nuget restore "${TESTS_DIR}/.."
	msbuild "${TESTS_DIR}/.."

build_efcore:
	dotnet publish --runtime osx-x64 "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.SQLite/*.csproj
	dotnet publish --runtime osx-x64 "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.MySQL/*.csproj
	dotnet publish --runtime osx-x64 "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.Npgsql/*.csproj
	dotnet publish --runtime osx-x64 "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.SqlServer/*.csproj

clean:
	find "${TESTS_DIR}/.." -name "bin" -print0 | xargs -0 rm -r
	find "${TESTS_DIR}/.." -name "obj" -print0 | xargs -0 rm -r

sqlite:
	rm -f DbContextValidation.sqlite3
	sqlite3 DbContextValidation.sqlite3 < SQL.Common/CreateTables.sql

pull_docker_images:
	docker pull postgres:10.5-alpine
	docker pull mysql/mysql-server:5.7
	docker pull genschsa/mssql-server-linux
	docker pull sath89/oracle-12c:r1
