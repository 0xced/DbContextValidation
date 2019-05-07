# Getting Makefile directory absolute path, see https://stackoverflow.com/questions/18136918/how-to-get-current-relative-directory-of-your-makefile/23324703#23324703
TESTS_DIR := $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

test: test_ef6 test_efcore

test_ef6: build_ef6
	mono ~/.nuget/packages/xunit.runner.console/2.4.1/tools/net452/xunit.console.exe "${TESTS_DIR}"/../DbContextValidation.Tests.EF6.*/bin/Debug/*/*.Tests.EF6.*.dll -noshadow -html Report.EF6.html -parallel all

test_efcore: build_efcore
	dotnet vstest "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.*/bin/Debug/*/osx-x64/publish/*.Tests.EFCore.*.dll --logger:"console;verbosity=normal" --parallel
	# Running tests in parallel (-parallel all) doesn't work, we get System.MissingMethodException but it works in parallel when running through dotnet vstest. Is this a bug in the xunit console runner?
	dotnet ~/.nuget/packages/xunit.runner.console/2.4.1/tools/netcoreapp2.0/xunit.console.dll "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.*/bin/Debug/*/osx-x64/publish/*.Tests.EFCore.*.dll -html Report.EFCore.html

build: build_ef6 build_efcore

build_ef6:
	nuget restore "${TESTS_DIR}/.."
	msbuild "${TESTS_DIR}/.."

build_efcore:
	for csproj in "${TESTS_DIR}"/../DbContextValidation.Tests.EFCore.*/*.csproj; do dotnet publish --runtime osx-x64 $${csproj}; done

clean:
	find "${TESTS_DIR}/.." -name "bin" -print0 | xargs -0 rm -r
	find "${TESTS_DIR}/.." -name "obj" -print0 | xargs -0 rm -r

sqlite:
	rm -f DbContextValidation.sqlite3
	sqlite3 DbContextValidation.sqlite3 < SQL.Common/CreateTables.sql

# Ordered by size
pull_docker_images:
	docker pull postgres:10.5-alpine
	docker pull jacobalberty/firebird:3.0.4
	docker pull mysql/mysql-server:5.7
	docker pull genschsa/mssql-server-linux
	docker pull quillbuilduser/oracle-18-xe
