# Klab.Toolkit.Messaging.Abstractions

## Overview

The `Klab.Toolkit.Messaging.Abstractions` package contains the interfaces and abstractions that define the messaging system in the `Klab.Toolkit.Messaging` package. This package is a core component of the Klab.Toolkit solution and provides a robust and flexible messaging system that allows different parts of the application to communicate with each other through events, requests, and streams. This package aims to promote decoupling and modularity by enabling mediator-based architecture.

## Purpose

The primary purpose of the `Klab.Toolkit.Messaging.Abstractions` package is to facilitate various forms of asynchronous communication within the application. By using this package, different components can publish and subscribe to events, send requests, and stream responses without needing to know about each other, thus promoting loose coupling and enhancing maintainability.

## Key Features

* Changeable Event Message Queue: The event message queue can be changed to any other implementation that implements the `IEventQueue` interface.
* Event Subscription: To subscribe to an event, you can use the `IMediator.Subscribe` method, passing in the event type and a callback function or register a handler class that implements the `IEventHandler` interface.
* Send Requests: The mediator also provides a `SendAsync` method that allows you to send a request to a handler and get a response back (like MediatR).
* Default an In-Memory Event Queue: The default implementation of the event queue is an in-memory queue that stores events in memory. This implementation is suitable for most applications, but you can replace it with a custom implementation if needed.

## Example Usage

See the test project `Klab.Toolkit.Messaging.Tests`.
