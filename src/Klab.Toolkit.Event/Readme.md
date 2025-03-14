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
* Save the event history to a defined file. See the `EventModuleConfiguration.EventLogFilePath` property.

## Example Usage

See the test project `Klab.Toolkit.Event.Tests`.

## Tipps

### Forbid to log sensitive data of a event

Each event will be logged as a json string.
If you want to forbid to log sensitive data of a event you can add the attribute `JsonIgnore` to the property.

Example: `MyEvent` will be logged without the property `Password`.

```csharp
public class MyEvent : IEvent
{
    public string Name { get; set; }

    [JsonIgnore]
    public string Password { get; set; }
}
```


