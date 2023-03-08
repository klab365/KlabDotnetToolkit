// Copyright (c) Klab
// The Klab licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Klab.Toolkit.Configuration;

/// <summary>
/// Interface for a property that can be validated and has a display name.
/// This Property can be used for a property of domain model.
/// </summary>
public interface IProperty
{
    /// <summary>
    /// Raised when the property changes
    /// </summary>
    event EventHandler? PropertyChanged;

    /// <summary>
    /// Gets display name of the property.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Validates the property.
    /// </summary>
    /// <returns></returns>
    bool IsValid();
}

/// <summary>
/// Interface for write read property.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IProperty<T> : IProperty
    where T : struct
{
    /// <summary>
    /// Gets or sets value of the property.
    /// </summary>
    T Value { get; set; }
}
