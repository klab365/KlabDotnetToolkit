using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Klab.Toolkit.Configuration;

/// <summary>
/// This Class is selectable property which contains a list of values and
/// always can have one selected value.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{Name}: {Value} ({Possibilities})")]
public class ChoicesProperty<T> : IProperty<T> where T : struct
{
    /// <inheritdoc/>
    public event EventHandler? PropertyChanged;

    /// <summary>
    /// Gets list of possible value for the property.
    /// </summary>
    public List<T> Possibilities { get; }

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
    /// Initializes a new instance of the <see cref="ChoicesProperty{T}"/> class.
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="possibilities"></param>
    public ChoicesProperty(string displayName, List<T> possibilities)
    {
        Name = displayName;
        Possibilities = possibilities;
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        if (Possibilities.Count == 0)
        {
            return true;
        }

        return Possibilities.Contains(Value);
    }
}
