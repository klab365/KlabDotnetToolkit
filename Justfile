set windows-shell := ["powershell"]

setup:
    dotnet tool install -g dotnet-format
    dotnet tool install -g dotnet-coverage

# add projects to solution
add-projects:
    dotnet sln add ls -r **/*.csproj

build configuration='Debug':
    dotnet build -c {{configuration}}

# clean the solution
[linux]
clean:
    echo "Cleaning..."
    dotnet clean
    find . -iname "bin" | xargs rm -rf
    find . -iname "obj" | xargs rm -rf

[windows]
clean:
    dotnet clean
    Get-ChildItem -Recurse -Filter bin | ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
    Get-ChildItem -Recurse -Filter obj | ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
    Get-ChildItem -Recurse -Filter tmp | ForEach-Object { Remove-Item $_.FullName -Recurse -Force }

# run the tests
test reportPath="./tmp" *args='':
    dotnet coverage \
        collect -f xml \
        -o {{reportPath}}/coverage.xml \
        "dotnet test --logger:junit;MethodFormat=Class;LogFilePath={{reportPath}}/{assembly}.results.xml {{args}}"

format *args='':
    dotnet format --verbosity diagnostic {{args}}

check-format:
    just format --verify-no-changes

restore:
    dotnet restore
