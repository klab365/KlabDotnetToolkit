// Copyright (c) Klab
// The Klab licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace Klab.Toolkit.Configuration;

/// <inheritdoc/>
[DebuggerDisplay("{DisplayName}: {Value}")]
public class Property<T> : IProperty<T>
    where T : struct
{
    private readonly List<Func<T, bool>> _isValidCallbacks;

    /// <inheritdoc/>
    public event EventHandler? PropertyChanged;

    /// <inheritdoc/>
    public string DisplayName { get; }

    private T _value;

    /// <inheritdoc/>
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Property{T}"/> class.
    /// Generate a property with self defined is valid callbacks.
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="value"></param>
    /// <param name="isValidCallbacks"></param>
    public Property(string displayName, T value, List<Func<T, bool>> isValidCallbacks)
    {
        DisplayName = displayName;
        Value = value;
        _isValidCallbacks = isValidCallbacks;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Property{T}"/> class.
    /// Generate a property without valid callbacks. IsValid will return always true.
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Property(string displayName, T value)
        : this(displayName, value, new())
    {
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        if (!_isValidCallbacks.Any())
        {
            return true;
        }

        return _isValidCallbacks.TrueForAll(x => x.Invoke(Value));
    }
}
