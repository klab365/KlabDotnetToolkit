# Klab.Toolkit.Event

## Overview

The `Klab.Toolkit.Event` package is a core component of the Klab.Toolkit solution. It provides a robust and flexible event handling system that allows different parts of the application to communicate with each other through events. This package aims to promote decoupling and modularity by enabling event-driven architecture.

## Purpose

The primary purpose of the `Klab.Toolkit.Event` package is to facilitate event-driven communication within the application. By using this package, different components can publish and subscribe to events without needing to know about each other, thus promoting loose coupling and enhancing maintainability.

## Key Features

* Changebale Event Message Queue: The event message queue can be changed to any other implementation that implements the `IEventQueue` interface.
* Event Subscription: To subscribe to an event, you can use the `EventBus.Subscribe` method, passing in the event type and a callback function or register a handler class that implements the `IEventHandler` interface.
* Send Requests: The EventBus also provides a `Send` method that allows you to send an request to a handler and get a response back (like MediatR).
* Default an In-Memory Event Queue: The default implementation of the event queue is an in-memory queue that stores events in memory. This implementation is suitable for most applications, but you can replace it with a custom implementation if needed.

## Example Usage

See the test project `Klab.Toolkit.Event.Tests`.
