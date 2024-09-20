# Klab.Toolkit.Common

## Overview

The `Klab.Toolkit.Common` package is a core component of the Klab.Toolkit solution. It provides a set of common classes and interfaces that are used across various projects within the solution. This package aims to promote code reuse, consistency, and flexibility by defining shared abstractions and utilities.

## Purpose

The primary purpose of the `Klab.Toolkit.Common` package is to define common classes and interfaces that can be utilized by other components of the Klab.Toolkit solution. By centralizing these definitions, the package helps to ensure that different parts of the solution can interact seamlessly and consistently.

## Key Features

- **Common Interfaces**: Defines interfaces that are used across the solution to promote consistency and flexibility. For example, the `ITimeProvider` interface abstracts the system time, making it easier to test time-dependent code.
- **Utility Classes**: Provides utility classes that offer common functionality needed by multiple components of the solution.
- **Abstractions**: Offers abstractions that help in decoupling components, making the system more modular and easier to maintain.

## Example Usage

### ITimeProvider Interface

The `ITimeProvider` interface is used to abstract the system time. This is particularly useful for testing purposes, as it allows you to control the flow of time in your tests.

### IRetryService Interface

The `IRetryService` interface defines a common way to perform retry logic in the system. By using this interface, you can easily add retry functionality to any part of the solution without duplicating code.

## Usage in Projects

To use the Klab.Toolkit.Common package in your projects call the Function on your Dependency Injection Container to register the services.

```csharp

services.AddKlabToolkitCommon();

```
