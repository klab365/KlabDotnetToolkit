# Changelog

All notable changes to this project will be documented in this file.

## Purpose

This file present the software status in form of a "Changelog".

## Scope

This document is valid within the scope of the work for all projects.

## 2.12.0

### Added

* **Result Extensions**: Add `Do` and `DoAsync` extension methods for Result pattern

## 2.11.1

### Changed

* Unwrap methods now return more detailed error messages including the error details when unwrapping fails

## 2.11.0

### Changed

* Update Results README documentation to reflect removal of implicit operators and provide migration examples

### Breaking Changes

* **Result Pattern**: Removed implicit operators for converting values to `Result<T>` and `Result<T>` to values
  - `Result<string> result = "value"` is no longer supported - use `Result<string> result = Result.Success("value")` instead
  - `string value = result` is no longer supported - use `string value = result.Unwrap()` or `result.Match()` instead
  - These changes improve type safety and make Result usage more explicit
  - Error to Result implicit conversion still remains available

## 2.10.0

### Changed

* Add unwrap and unwrap async methods to ResultExtensions for better error handling
* Refactor event module registration to use AddEventModule method
* Update Error class to not have severity field anymore

## 2.9.0

### Changed

* Update all Readme files to reflect the latest changes in the projects
* ShouldLogEvents property in EventModule is defaulted to false
* Remove Ensure method from Result class

## 2.8.2

### Changed

* Update ResultExtensions functions with Async for non async methods

## 2.8.1

### Changed

* Update ResultExtensions functions with OnFailure

## 2.8.0

### Changed

* Move interface IEvent to abstract class EventBase for common properties

## 2.7.3

### Changed

* Update ResultExtensions functions

## 2.7.2

### Changed

* Update ResultExtensions functions

## 2.7.1

### Changed

* Update some functions in the result extensions

## 2.7.0

### Added

* Add ResultExtensions to the Klab.Toolkit.Results project

## 2.6.0

* Add some implict operators to the Result class
* Remove IResult and IError interfaces, because they are not needed anymore

## 2.5.1

### Changed

* Process Events in the EventBus in sequential order

## 2.5.0

* Changed
  * Process Events in the EventBus in a separate task to avoid blocking processing of events

## 2.4.1

* Remove simple IRequest and IRequestHandler. Every Request must contain a response.

## 2.4.0

* Add StreamRequest to the EventBus
* Remove unessesary function calls of the eventbus

## 2.3.0

* Split Event Module to Abstractions and Implementation
* Update Result to IResult for more flexibility

## 2.2.4

* Make IRequest more compile time safe (better type checking)

## 2.2.3

* Integrate InMemoryMessageQueue to the main Event Project

## 2.2.2

* Add Send Functionality to the Eventbus

## 2.2.1

* Remove unessesary classes, fields, etc.

## 2.2.0

* Add functtionality to the eventbus to register local function as event handler

## 2.1.0

* Optimize Result class (non nullable)
* Add more extension methods

## 2.0.0 - 28.12.2023

### Added

* Created new projects:
  * Klab.Toolkit.Common: Common services and classes
  * Klab.Toolkit.Configuration: Classes for configuration things
  * Klab.Toolkit.DI: Classes for dependency injection
  * Klab.Toolkit.ExtensionMethods: Extension methods for common classes like string, enum, etc.
  * Klab.Toolkit.Results: Classes for results
  * Klab.Toolkit.ValueObjects: Classes for value objects
