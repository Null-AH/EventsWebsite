using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Services
{
    using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonStringEnumMemberConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private readonly Dictionary<T, string> _toString;
    private readonly Dictionary<string, T> _fromString;

    public JsonStringEnumMemberConverter()
    {
        _toString = new Dictionary<T, string>();
        _fromString = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        var type = typeof(T);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumValue = (T)field.GetValue(null);
            var enumMemberAttr = field.GetCustomAttribute<EnumMemberAttribute>();
            var name = enumMemberAttr?.Value ?? field.Name;

            _toString[enumValue] = name;
            if (!_fromString.ContainsKey(name))
                _fromString[name] = enumValue;

            // also accept plain enum name
            if (!_fromString.ContainsKey(field.Name))
                _fromString[field.Name] = enumValue;
        }
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();

        var s = reader.GetString() ?? string.Empty;

        if (_fromString.TryGetValue(s, out var val))
            return val;

        throw new JsonException($"Unknown enum value '{s}' for enum '{typeof(T).Name}'.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (_toString.TryGetValue(value, out var name))
            writer.WriteStringValue(name);
        else
            writer.WriteStringValue(value.ToString());
    }
}

public class EnumMemberConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(JsonStringEnumMemberConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

}