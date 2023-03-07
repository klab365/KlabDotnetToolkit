using System.ComponentModel;
using System.Reflection;

namespace Klab.Toolkit.Common.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Will get the string value for a given enums value, this will
    /// only work if you assign the StringValue attribute to
    /// the items in your enum.
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetEnumDescription<T>(this T enumValue) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            return string.Empty;
        }

        string? description = enumValue.ToString();
        if (description is null)
        {
            return string.Empty;
        }

        FieldInfo? fieldInfo = enumValue.GetType().GetField(enumValue.ToString() ?? string.Empty);
        if (fieldInfo is null)
        {
            return string.Empty;
        }

        object[] attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
        if (attrs.Length > 0)
        {
            description = ((DescriptionAttribute)attrs[0]).Description;
        }

        return description;
    }

    /// <summary>
    /// Will get the name of the enum and the description from the description attribute
    /// </summary>
    /// <code>
    /// public enum Test : int
    /// {
    ///     [DescriptionAttribute("This is a test")]
    ///     EnumA = 10
    /// }
    /// 
    /// Dictionary<string, string> table = EnumExtensions.GetDictionaryWithEnumNameAndDescription&lt;Test&gt;();
    /// </code>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Dictionary<string, string> GetDictionaryWithEnumNameAndDescription<T>()
    {
        FieldInfo[] fieldInfos = typeof(T).GetFields();

        Dictionary<string, string> table = new();
        foreach (FieldInfo info in fieldInfos)
        {
            DescriptionAttribute? descriptionAttribute = info.GetCustomAttribute<DescriptionAttribute>(inherit: true);
            if (descriptionAttribute is null)
            {
                return new();
            }

            table.Add(info.Name, descriptionAttribute.Description);
        }

        return table;
    }
}
