using System;
using System.Diagnostics.CodeAnalysis;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Messaging Errors
/// </summary>
[ExcludeFromCodeCoverage]
public static class MessagingErrors
{
    /// <summary>
    /// Keys
    /// </summary>
    public static class Keys
    {
        /// <summary>
        /// Event Handler Not Found Key
        /// </summary>
        public const string EventHandlerNotFoundKey = "EventHandlerNotFound";

        /// <summary>
        /// Generic Event Error Key
        /// </summary>
        public const string GenericEventErrorKey = "GenericEventError";
    }

    /// <summary>
    /// Event Handler Not Found Error
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns></returns>
    public static IError EventHandlerNotFound(Type eventType) => Error.Create(
        Keys.EventHandlerNotFoundKey,
        message: $"No event handler found for event type {eventType}",
        advice: "Make sure you registered the event handler in the DI container. If not desired, then ignore this message.");
}
