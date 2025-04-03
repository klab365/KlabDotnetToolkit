# Klab.Toolkit.Results

## Purpose

This project is a part of the Klab.Toolkit solution and is used to return results from methods. The result can be a success or a failure and can contain a value or an error message. The error is a interface and can be extended. Default Error Types are: `Error`.

## Features

- `Result<T>`: A class that represents the result of an operation. It can be either a success or a failure.
- `Result`: A class that represents the result of an operation without a value. It can be either a success or a failure.
- `Error`: An interface that represents an error. It can be extended to create custom error types.
- `ResultExtensions`: A static class that contains extension methods for `Result<T>` and `Result`. It provides methods to create success and failure results, and to convert between the two types, and also implement some functional programming concepts like `Map`, `Bind`, and `Tap`, etc.

