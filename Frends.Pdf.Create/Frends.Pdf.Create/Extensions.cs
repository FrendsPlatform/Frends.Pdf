using System;

namespace Frends.Pdf.Create;

/// <summary>Extension methods used internally by the Pdf task.</summary>
static class Extensions
{
    /// <summary>Converts an enum value to the equivalent value in a different enum type.</summary>
    public static TEnum ConvertEnum<TEnum>(this Enum source)
    {
        return (TEnum)Enum.Parse(typeof(TEnum), source.ToString(), true);
    }
}
