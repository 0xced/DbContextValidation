# Getting Makefile directory absolute path, see https://stackoverflow.com/questions/18136918/how-to-get-current-relative-directory-of-your-makefile/23324703#23324703
TESTS_DIR := $(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

test: test_ef6 test_efcore

test_ef6: build_ef6
	mono ~/.nuget/packages/xunit.runner.console/2.4.1/tools/net452/xunit.console.exe "${TESTS_DIR}"/EF6/*/bin/Debug/*/*.Tests.EF6.*.dll -verbose -noshadow -html Report.EF6.html

test_efcore: build_efcore
	mkdir -p TestResults # https://github.com/microsoft/vstest/issues/2318#issuecomment-614242976
	dotnet vstest "${TESTS_DIR}"/EFCore2/*/bin/Release/*/publish/*.Tests.EFCore*.dll --logger:"console;verbosity=normal" --logger:"html;LogFileName=${TESTS_DIR}/Report.EFCore2.html" --parallel
	dotnet vstest "${TESTS_DIR}"/EFCore3/*/bin/Release/*/publish/*.Tests.EFCore*.dll --logger:"console;verbosity=normal" --logger:"html;LogFileName=${TESTS_DIR}/Report.EFCore3.html" --parallel
	rmdir TestResults
	# Running tests in parallel (-parallel all) doesn't work, we get System.MissingMethodException but it works in parallel when running through dotnet vstest. Is this a bug in the xunit console runner?
	# dotnet ~/.nuget/packages/xunit.runner.console/2.4.1/tools/netcoreapp2.0/xunit.console.dll "${TESTS_DIR}"/EFCore*/*/bin/Debug/*/osx-x64/publish/*.Tests.EFCore*.dll -html Report.EFCore.html

build: build_ef6 build_efcore

build_ef6:
	nuget restore "${TESTS_DIR}/.."
	msbuild "${TESTS_DIR}/.."

build_efcore:
	for csproj in "${TESTS_DIR}"/EFCore*/*/*.csproj; do dotnet publish --configuration Release $${csproj}; done

clean:
	find "${TESTS_DIR}/.." -name "bin" -print0 | xargs -0 rm -r
	find "${TESTS_DIR}/.." -name "obj" -print0 | xargs -0 rm -r

sqlite:
	rm -f DbContextValidation.sqlite3
	cat SQL.Common/* | sqlite3 DbContextValidation.sqlite3

# Ordered by size
pull_docker_images:
	docker pull postgres:10.5-alpine
	docker pull jacobalberty/firebird:3.0.4
	docker pull mysql/mysql-server:5.7
	docker pull genschsa/mssql-server-linux
	docker pull wnameless/oracle-xe-11g-r2
