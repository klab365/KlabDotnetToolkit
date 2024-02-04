using System;
using System.ComponentModel;
using System.Reflection;

namespace Klab.Toolkit.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="DescriptionAttribute"/>.
/// </summary>
public static class DescriptionAttributeExtensions
{
    /// <summary>
    /// Get the description of the specified type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetDescription(this Type type)
    {
        DescriptionAttribute[] descriptionAttribute = (DescriptionAttribute[])type.GetCustomAttributes<DescriptionAttribute>();
        if (descriptionAttribute is null || descriptionAttribute.Length == 0)
        {
            return string.Empty;
        }

        return descriptionAttribute[0].Description;
    }
}
