using System;
using System.Collections.Generic;
using System.Linq;

namespace Klab.Toolkit.Common.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Type"/> objects.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the specified open concretion type could close to the specified closed interface type.
    /// </summary>
    /// <param name="openConcretion">The open concretion type.</param>
    /// <param name="closedInterface">The closed interface type.</param>
    /// <returns><c>true</c> if the open concretion type could close to the closed interface type; otherwise, <c>false</c>.</returns>
    public static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
    {
        Type openInterface = closedInterface.GetGenericTypeDefinition();
        Type[] arguments = closedInterface.GenericTypeArguments;

        Type[] concreteArguments = openConcretion.GenericTypeArguments;
        return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
    }

    /// <summary>
    /// Determines whether the specified plugged type can be cast to the specified plugin type.
    /// </summary>
    /// <param name="pluggedType">The plugged type.</param>
    /// <param name="pluginType">The plugin type.</param>
    /// <returns><c>true</c> if the plugged type can be cast to the plugin type; otherwise, <c>false</c>.</returns>
    public static bool CanBeCastTo(this Type pluggedType, Type pluginType)
    {
        if (pluggedType == null)
        {
            return false;
        }

        if (pluggedType == pluginType)
        {
            return true;
        }

        return pluginType.IsAssignableFrom(pluggedType);
    }

    /// <summary>
    /// Finds the interfaces implemented by the specified plugged type that close to the specified template type.
    /// </summary>
    /// <param name="pluggedType">The plugged type.</param>
    /// <param name="templateType">The template type.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/> objects representing the interfaces that close to the template type.</returns>
    public static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
    {
        return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
    }

    /// <summary>
    /// Finds the interfaces implemented by the specified plugged type that close to the specified template type.
    /// </summary>
    /// <param name="pluggedType">The plugged type.</param>
    /// <param name="templateType">The template type.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/> objects representing the interfaces that close to the template type.</returns>
    public static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
    {
        if (pluggedType == null)
        {
            yield break;
        }

        if (!pluggedType.IsConcrete())
        {
            yield break;
        }

        if (templateType.IsInterface)
        {
            IEnumerable<Type> interfaceTypes = pluggedType.GetInterfaces().Where(type => type.IsGenericType && (type.GetGenericTypeDefinition() == templateType));
            foreach (Type? interfaceType in interfaceTypes)
            {
                yield return interfaceType;
            }
        }
        else if (pluggedType.BaseType!.IsGenericType &&
                (pluggedType.BaseType!.GetGenericTypeDefinition() == templateType))
        {
            yield return pluggedType.BaseType!;
        }

        if (pluggedType.BaseType == typeof(object))
        {
            yield break;
        }

        foreach (Type interfaceType in FindInterfacesThatClosesCore(pluggedType.BaseType!, templateType))
        {
            yield return interfaceType;
        }
    }

    /// <summary>
    /// Determines whether the specified type is an open generic type.
    /// </summary>
    /// <param name="typeInfo">The type information.</param>
    /// <returns><c>true</c> if the type is an open generic type; otherwise, <c>false</c>.</returns>
    public static bool IsOpenGeneric(this Type typeInfo)
    {
        return typeInfo.IsGenericType && typeInfo.ContainsGenericParameters;
    }

    /// <summary>
    /// Determines whether the specified type is a concrete type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if the type is a concrete type; otherwise, <c>false</c>.</returns>
    public static bool IsConcrete(this Type type)
    {
        return !type.IsAbstract && !type.IsInterface;
    }
}
