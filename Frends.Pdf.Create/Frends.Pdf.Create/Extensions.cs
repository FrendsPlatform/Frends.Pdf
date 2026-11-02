using System;

namespace Frends.Pdf.Create;

/// <summary>Extension methods used internally by the Pdf task.</summary>
internal static class Extensions
{
    /// <summary>Converts an enum value to the equivalent value in a different enum type.</summary>
    /// <typeparam name="TEnum">Target enum type.</typeparam>
    /// <param name="source">Enum value to convert.</param>
    /// <returns>The equivalent value in the target enum type.</returns>
    public static TEnum ConvertEnum<TEnum>(this Enum source)
    {
        return (TEnum)Enum.Parse(typeof(TEnum), source.ToString(), true);
    }
}
