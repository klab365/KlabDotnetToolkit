build:
	@dotnet build

clean:
	@dotnet clean
	@find . -iname "bin" | xargs rm -rf
	@find . -iname "obj" | xargs rm -rf

test:
	@dotnet test

format:
	@dotnet format -v diag
