# build the solution
build *args:
    echo "Building..."
    dotnet build {{args}}

# clean the solution
clean:
    echo "Cleaning..."
    dotnet clean
    find . -iname "bin" | xargs rm -rf
    find . -iname "obj" | xargs rm -rf

# run the tests
test:
    dotnet test

# format the code
format:
    dotnet format -v diag
