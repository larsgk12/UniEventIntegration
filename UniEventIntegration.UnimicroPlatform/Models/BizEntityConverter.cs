namespace UniEventIntegration.Models;

public class BizEntityConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var attr = typeToConvert.GetCustomAttribute<BizEntityAttribute>();
        return attr is not null;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var instanceType = typeof(BizEntityConverter<>).MakeGenericType(typeToConvert);
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            instanceType,
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null)!;

        return converter;
    }
}

#pragma warning disable CS9113 // Parameter is unread.
internal sealed class BizEntityConverter<T>(JsonSerializerOptions options) : JsonConverter<T>
#pragma warning restore CS9113 // Parameter is unread.
    where T : IBizEntity, new()
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var result = new T();
        var customValues = false;
        var clrName = typeToConvert.Name;
        var lowerClrName = clrName.ToLowerInvariant();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (!customValues) break;
                customValues = false;
                continue;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            var propName = reader.GetString();
            reader.Read();
            if (string.IsNullOrWhiteSpace(propName)) 
            { 
                _ = reader.TrySkip(); 
                continue; 
            }

            if (propName.Equals("CustomValues", StringComparison.OrdinalIgnoreCase))
            {
                if (reader.TokenType == JsonTokenType.Null) continue;
                customValues = true;
                continue;
            }

            var prop = typeToConvert.GetProperty(propName) 
                ?? typeToConvert.GetProperties().FirstOrDefault(
                    p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == propName
                );

            if (prop is null)
            {
                // Try stripping type name prefix
                var stripped = StripPrefix(propName, clrName, lowerClrName);
                if (stripped is not null)
                {
                    prop = typeToConvert.GetProperty(stripped)
                        ?? typeToConvert.GetProperties().FirstOrDefault(
                            p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == stripped
                        );
                }
            }

            if (prop is null)
            {
                _ = reader.TrySkip();
                continue;
            }

            var value = JsonSerializer.Deserialize(ref reader, prop.PropertyType);
            prop.SetValue(result, value);
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var props = value.GetType().GetProperties();

        var customFields = 0;
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<BizCustomFieldAttribute>();
            if (attr is not null)
            {
                customFields++;
                continue;
            }
            writer.WritePropertyName(prop.Name);
            var propVal = prop.GetValue(value);
            JsonSerializer.Serialize(writer, propVal, prop.PropertyType, options);
        }
        if (customFields > 0)
        {
            writer.WritePropertyName("CustomValues");
            writer.WriteStartObject();
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<BizCustomFieldAttribute>();
                if (attr is null) continue;
                writer.WritePropertyName(prop.Name);
                var propVal = prop.GetValue(value);
                JsonSerializer.Serialize(writer, propVal, prop.PropertyType, options);
            }
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    /// <summary>
    /// Removes the specified type name prefix from the beginning of a property name, if present.
    /// </summary>
    /// <remarks>The method checks for the type name prefix in the original, lowercase, and uppercase forms.
    /// If the property name does not start with any of these prefixes, the method returns null.</remarks>
    /// <param name="propName">The property name to process. This value is checked for the presence of the type name prefix in various casing
    /// styles.</param>
    /// <param name="typeName">The type name to remove as a prefix from the property name. The comparison is case-sensitive and also considers
    /// uppercase and lowercase variants.</param>
    /// <param name="typeNameLower">The lowercase form of the type name. Used to match property names that use a lowercase prefix.</param>
    /// <returns>A substring of the property name with the type name prefix removed if a match is found; otherwise, null.</returns>
    private static string? StripPrefix(string propName, string typeName, string typeNameLower)
    {
        // Exact CLR type name prefix: WageTypeID => ID
        if (propName.StartsWith(typeName, StringComparison.Ordinal))
            return propName[typeName.Length..];

        // Lower-cased type name prefix: wagetypeID or wagetypeId => ID
        if (propName.StartsWith(typeNameLower, StringComparison.Ordinal))
            return propName[typeNameLower.Length..];

        // Also consider PascalCase collapsed prefix (e.g., WageType -> WAGETYPE in some feeds)
        var typeNameUpper = typeName.ToUpperInvariant();
        return propName.StartsWith(typeNameUpper, StringComparison.Ordinal)
            ? propName[typeNameUpper.Length..]
            : null;
    }
}
