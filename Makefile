clean:
	dotnet clean
	find . -iname "bin" | xargs rm -rf
	find . -iname "obj" | xargs rm -rf
