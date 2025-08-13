
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace EventApi.Services
{

    public static class EnumUtils
    {
        // cache: Type -> map of accepted string -> enum value
        private static readonly ConcurrentDictionary<Type, Dictionary<string, object>> Cache =
            new ConcurrentDictionary<Type, Dictionary<string, object>>();

        public static bool TryParseEnumMember<TEnum>(string input, out TEnum value) where TEnum : struct, Enum
        {
            value = default;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // <-- FIX: use lambda to call generic BuildMap<TEnum>()
            var map = Cache.GetOrAdd(typeof(TEnum), _ => BuildMap<TEnum>());

            // try direct match (case-insensitive)
            if (map.TryGetValue(input.Trim(), out var v))
            {
                value = (TEnum)v;
                return true;
            }

            // try normalized match (remove spaces/hyphens/underscores)
            var norm = Normalize(input);
            if (map.TryGetValue(norm, out v))
            {
                value = (TEnum)v;
                return true;
            }

            // fallback to normal Enum.TryParse (accepts "CheckInStaff" etc.)
            if (Enum.TryParse<TEnum>(input, true, out var parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }

        public static TEnum ParseEnumMember<TEnum>(string input) where TEnum : struct, Enum
        {
            if (TryParseEnumMember<TEnum>(input, out var v)) return v;
            throw new ArgumentException($"Unknown value '{input}' for enum {typeof(TEnum).Name}");
        }

        private static Dictionary<string, object> BuildMap<TEnum>() where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumValue = (TEnum)field.GetValue(null)!;
                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>()?.Value;
                var name = field.Name;

                AddIfMissing(dict, name, enumValue);                 // "CheckInStaff"
                if (!string.IsNullOrEmpty(enumMember))
                    AddIfMissing(dict, enumMember, enumValue);       // "Check-In Staff"

                // normalized forms without spaces/hyphens/underscores:
                AddIfMissing(dict, Normalize(name), enumValue);     // "checkinstaff"
                if (!string.IsNullOrEmpty(enumMember))
                    AddIfMissing(dict, Normalize(enumMember), enumValue);
            }

            return dict;
        }

        private static void AddIfMissing<TEnum>(Dictionary<string, object> dict, string key, TEnum value) where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!dict.ContainsKey(key))
                dict[key] = value!;
        }

        private static string Normalize(string s)
        {
            return new string(s.Where(c => c != ' ' && c != '-' && c != '_').ToArray()).ToLowerInvariant();
        }
    }



public static class EnumExtensions
{
    // Cache: enum Type -> map of underlying numeric value -> string (EnumMember or name)
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<long, string>> _cache
        = new();

    public static string GetEnumMemberValue<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var typeMap = _cache.GetOrAdd(type, t =>
        {
            var map = new ConcurrentDictionary<long, string>();
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumVal = (TEnum)f.GetValue(null)!;
                var underlying = Convert.ToInt64(enumVal);
                var enumMember = f.GetCustomAttribute<EnumMemberAttribute>()?.Value;
                map[underlying] = string.IsNullOrEmpty(enumMember) ? f.Name : enumMember;
            }
            return map;
        });

        var key = Convert.ToInt64(value);
        return typeMap.TryGetValue(key, out var s) ? s : value.ToString();
    }
}



}