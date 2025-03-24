# Changelog

All notable changes to this project will be documented in this file.

## Purpose

This file present the software status in form of a "Changelog".

## Scope

This document is valid within the scope of the work for all projects.

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
