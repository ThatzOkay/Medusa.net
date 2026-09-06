using System.Collections.Generic;
using System.Linq;
using Abstractions.Extractors.Unity.Models;
using AssetsTools.NET;

namespace Abstractions.Extractors.Unity.Helpers;

/// <summary>Generic AssetTypeValueField tree-walking utilities.</summary>
public static class ReflectionHelper
{
    private static readonly string[] NameCandidates = ["name", "m_Name", "m_ClassName", "m_ObjectHideFlags"];

    /// <summary>UTF-8 decoding that silently drops invalid byte sequences instead of substituting
    /// U+FFFD.</summary>
    public static readonly System.Text.Encoding Utf8IgnoreErrors = System.Text.Encoding.GetEncoding(
        "utf-8",
        System.Text.EncoderFallback.ReplacementFallback,
        new System.Text.DecoderReplacementFallback(""));

    /// <summary>Converts a deserialized field tree into a plain object graph (Dictionary/List/primitives)
    /// suitable for System.Text.Json.</summary>
    public static object? ToPlainObject(AssetTypeValueField field)
    {
        var value = field.Value;
        if (value is not null && value.ValueType != AssetValueType.None)
        {
            switch (value.ValueType)
            {
                case AssetValueType.Array:
                    return field.Children.Select(ToPlainObject).ToList();
                case AssetValueType.ByteArray:
                    return field.AsByteArray;
                case AssetValueType.String:
                    return field.AsString;
                case AssetValueType.Bool:
                    return field.AsBool;
                case AssetValueType.Int8:
                    return field.AsSByte;
                case AssetValueType.UInt8:
                    return field.AsByte;
                case AssetValueType.Int16:
                    return field.AsShort;
                case AssetValueType.UInt16:
                    return field.AsUShort;
                case AssetValueType.Int32:
                    return field.AsInt;
                case AssetValueType.UInt32:
                    return field.AsUInt;
                case AssetValueType.Int64:
                    return field.AsLong;
                case AssetValueType.UInt64:
                    return field.AsULong;
                case AssetValueType.Float:
                    return field.AsFloat;
                case AssetValueType.Double:
                    return field.AsDouble;
                default:
                    return field.AsObject;
            }
        }

        if (field.Children.Count == 0) return null;

        // A "vector"/"map" wrapper (List<T>/Dictionary<K,V> in the original C# class) has exactly one
        // child literally named "Array" holding the elements - unwrap it so it comes out as a plain
        // JSON array/list of pairs instead of an extra {"Array": [...]} nesting level.
        if (field.Children.Count == 1 && field.Children[0].FieldName == "Array")
            return ToPlainObject(field.Children[0]);

        var dict = new Dictionary<string, object?>();
        foreach (var child in field.Children)
            dict[child.FieldName] = ToPlainObject(child);
        return dict;
    }

    /// <summary>True if this field is a PPtr (m_FileID/m_PathID pair) pointing at a non-null object.</summary>
    public static bool TryGetPPtr(AssetTypeValueField field, out long pathId, out int fileId)
    {
        pathId = 0;
        fileId = 0;

        var pathIdField = field.Get("m_PathID");
        if (pathIdField.IsDummy) return false;

        pathId = pathIdField.AsLong;
        if (pathId == 0) return false;

        var fileIdField = field.Get("m_FileID");
        fileId = fileIdField.IsDummy ? 0 : fileIdField.AsInt;
        return true;
    }

    /// <summary>Reads a top-level string field, returning false if it's absent/empty.</summary>
    public static bool TryGetNonEmptyString(AssetTypeValueField field, string name, out string value)
    {
        value = "";
        var child = field.Get(name);
        if (child.IsDummy) return false;

        if (child.Value?.ValueType == AssetValueType.String)
        {
            value = child.AsString.Trim();
        }
        else if (child.Value?.ValueType == AssetValueType.ByteArray)
        {
            value = System.Text.Encoding.UTF8.GetString(child.AsByteArray).Trim();
        }
        else
        {
            return false;
        }

        return !string.IsNullOrEmpty(value);
    }

    /// <summary>Looks at a handful of well-known name fields before falling back to
    /// "{objType}_{pathId}".</summary>
    public static string GetObjectName(AssetTypeValueField baseField, string objType, long pathId)
    {
        foreach (var candidate in NameCandidates)
        {
            if (TryGetNonEmptyString(baseField, candidate, out var name))
                return FileNameSanitizer.Sanitize(name);
        }

        var selfPathId = baseField.Get("m_PathID");
        if (!selfPathId.IsDummy && selfPathId.AsLong != 0)
            return $"Object_{selfPathId.AsLong}";

        return $"{objType}_{pathId}";
    }

    /// <summary>Reads the m_StreamData child if present.</summary>
    public static StreamingInfo? TryGetStreamingInfo(AssetTypeValueField baseField)
    {
        var streamData = baseField.Get("m_StreamData");
        if (streamData.IsDummy) return null;

        return new StreamingInfo
        {
            Offset = streamData.Get("offset") is { IsDummy: false } offset ? offset.AsLong : 0,
            Size = streamData.Get("size") is { IsDummy: false } size ? size.AsLong : 0,
            Path = streamData.Get("path") is { IsDummy: false } path ? path.AsString : "",
        };
    }
}
