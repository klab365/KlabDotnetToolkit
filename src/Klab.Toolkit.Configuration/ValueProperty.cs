using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Klab.Toolkit.Configuration;

/// <summary>
/// This class is a property contain a value and can be validate. These validations
/// are defined by the caller. The name can be used for the presentation.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{Name}: {Value}")]
public class ValueProperty<T> : IProperty<T> where T : struct
{
    private readonly List<Func<T, bool>> _isValidCallbacks;

    /// <inheritdoc/>
    public event EventHandler? PropertyChanged;

    /// <inheritdoc/>
    public string Name { get; }

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
    /// Create a new instance of the <see cref="ValueProperty{T}"/> class.
    /// Generate a property with self defined is valid callbacks.
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="value"></param>
    /// <param name="isValidCallbacks"></param>
    public ValueProperty(string displayName, T value, List<Func<T, bool>> isValidCallbacks)
    {
        Name = displayName;
        Value = value;
        _isValidCallbacks = isValidCallbacks;
    }

    /// <summary>
    /// Create a new instance of the <see cref="ValueProperty{T}"/> class without validation.
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ValueProperty(string displayName, T value) : this(displayName, value, [])
    {
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        if (_isValidCallbacks.Count == 0)
        {
            return true;
        }

        return _isValidCallbacks.TrueForAll(x => x.Invoke(Value));
    }
}
