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

        /// <summary>
        /// Operation cancelled error key.
        /// </summary>
        public const string OperationCancelledKey = "OperationCancelled";
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

    /// <summary>
    /// Gets an error indicating that an operation was cancelled.
    /// </summary>
    /// <returns>The cancellation error.</returns>
    public static IError OperationCancelled() => Error.Create(
        Keys.OperationCancelledKey,
        "The messaging operation was cancelled.");
}
