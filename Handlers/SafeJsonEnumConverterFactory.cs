using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LongLargo.Handlers;

/// <summary>
/// Safe enum parser. Needed because I don't want to completely destroy PlaylistInfo just because user wrote 'TimeDust'
/// </summary>
public class SafeJsonEnumConverterFactory : JsonConverterFactory
{
    // Determines if the type to convert is an Enum
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    // Creates the specialized converter for the specific Enum type
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(SafeJsonEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class SafeJsonEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle string values in JSON
        if (reader.TokenType == JsonTokenType.String)
        {
            string enumString = reader.GetString();
            if (Enum.TryParse<TEnum>(enumString, ignoreCase: true, out var result))
            {
                return result;
            }
        }
        // Handle numeric values in JSON
        else if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int intValue))
            {
                if (Enum.IsDefined(typeof(TEnum), intValue))
                {
                    return (TEnum)(object)intValue;
                }
            }
        }

        // Safe Fallback: Returns the first defined element (usually index 0, e.g., 'Unknown' or 'None')
        LLogger.Error($"Unknown enum {nameof(TEnum)} value: {reader.GetString()}");
        return default(TEnum);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        // Serializes the enum cleanly as a string
        writer.WriteStringValue(value.ToString());
    }
}