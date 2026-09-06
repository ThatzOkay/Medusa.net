using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Services;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using SkiaSharp;

namespace Abstractions.Extractors.Unity;

/// <summary>
/// Extracts Texture2D and Sprite objects to PNG, using AssetsTools.NET.Texture for the format decode
/// plus a hand-rolled sprite crop. Unity stores texture rows bottom-to-top, so decoded pixels are
/// flipped before encoding.
/// </summary>
public sealed class TextureExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["Texture2D", "Sprite"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        return context.ObjectType switch
        {
            "Texture2D" => await ExtractTexture2DAsync(context, cancellationToken),
            "Sprite" => await ExtractSpriteAsync(context, cancellationToken),
            _ => false,
        };
    }

    private static async Task<bool> ExtractTexture2DAsync(ExtractionContext context, CancellationToken cancellationToken)
    {
        try
        {
            var (pixels, width, height) = DecodeTopDownRgba(context.FileInstance, context.BaseField, context.DependencyIndex);
            if (pixels is null) return false;

            WritePng(pixels, width, height, context.OutputPathNoExtension + ".png");

            var baseField = context.BaseField;
            var texInfo = new Dictionary<string, object?>
            {
                ["width"] = width,
                ["height"] = height,
                ["format"] = baseField.Get("m_TextureFormat") is { IsDummy: false } fmt
                    ? ((TextureFormat)fmt.AsInt).ToString()
                    : "Unknown",
                ["mip_count"] = baseField.Get("m_MipCount") is { IsDummy: false } mc ? mc.AsInt : 1,
                ["is_readable"] = baseField.Get("m_IsReadable") is { IsDummy: false } r && r.AsBool,
                ["streaming_info"] = context.StreamingInfo,
            };
            await ObjectSerializer.WriteJsonAsync(texInfo, context.OutputPathNoExtension + "_info.json", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Unpacked sprites carry their own m_RD.texture PPtr straight to a Texture2D and crop with
    /// m_Rect. Atlas-packed sprites (the common case when Sprite Packer/Atlas is on) instead leave
    /// m_RD.texture at zero; the real texture + crop rect live in the SpriteAtlas object referenced
    /// by m_SpriteAtlas (frequently a *different* .bundle file entirely), keyed by this sprite's
    /// m_RenderDataKey inside the atlas's m_RenderDataMap.
    /// </summary>
    private static async Task<bool> ExtractSpriteAsync(ExtractionContext context, CancellationToken cancellationToken)
    {
        try
        {
            var baseField = context.BaseField;

            var resolved =
                TryResolveDirectTexture(context, baseField) ??
                TryResolveAtlasTexture(context, baseField);

            if (resolved is not { } renderData) return false;
            var (textureFileInstance, textureBaseField, rectField) = renderData;

            var (pixels, texWidth, texHeight) = DecodeTopDownRgba(textureFileInstance, textureBaseField, context.DependencyIndex);
            if (pixels is null) return false;

            var rectX = (int)rectField.Get("x").AsFloat;
            var rectY = (int)rectField.Get("y").AsFloat;
            var rectWidth = (int)rectField.Get("width").AsFloat;
            var rectHeight = (int)rectField.Get("height").AsFloat;
            if (rectWidth <= 0 || rectHeight <= 0) return false;

            var cropTop = texHeight - (rectY + rectHeight);
            var cropped = Crop(pixels, texWidth, rectX, cropTop, rectWidth, rectHeight);
            WritePng(cropped, rectWidth, rectHeight, context.OutputPathNoExtension + ".png");

            var pivot = baseField.Get("m_Pivot");
            var spriteInfo = new Dictionary<string, object?>
            {
                ["rect_x"] = rectX,
                ["rect_y"] = rectY,
                ["rect_width"] = rectWidth,
                ["rect_height"] = rectHeight,
                ["pivot_x"] = pivot.Get("x") is { IsDummy: false } px ? px.AsFloat : 0.5f,
                ["pivot_y"] = pivot.Get("y") is { IsDummy: false } py ? py.AsFloat : 0.5f,
                ["pixels_per_unit"] = baseField.Get("m_PixelsPerUnit") is { IsDummy: false } ppu ? ppu.AsFloat : 100f,
            };
            await ObjectSerializer.WriteJsonAsync(spriteInfo, context.OutputPathNoExtension + "_info.json", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static (AssetsFileInstance FileInstance, AssetTypeValueField TextureBaseField, AssetTypeValueField RectField)? TryResolveDirectTexture(
        ExtractionContext context, AssetTypeValueField spriteBaseField)
    {
        var texturePptr = spriteBaseField.Get("m_RD").Get("texture");
        if (!context.Loader.TryResolvePPtr(context.FileInstance, texturePptr, context.DependencyIndex, out var textureBaseField, out var textureFileInstance) ||
            textureBaseField is null || textureFileInstance is null)
            return null;

        return (textureFileInstance, textureBaseField, spriteBaseField.Get("m_Rect"));
    }

    private static (AssetsFileInstance FileInstance, AssetTypeValueField TextureBaseField, AssetTypeValueField RectField)? TryResolveAtlasTexture(
        ExtractionContext context, AssetTypeValueField spriteBaseField)
    {
        var atlasPptr = spriteBaseField.Get("m_SpriteAtlas");
        if (!context.Loader.TryResolvePPtr(context.FileInstance, atlasPptr, context.DependencyIndex, out var atlasBaseField, out var atlasFileInstance) ||
            atlasBaseField is null || atlasFileInstance is null)
            return null;

        var renderDataKey = spriteBaseField.Get("m_RenderDataKey");
        var spriteAtlasData = BundleLoader.ArrayElements(atlasBaseField.Get("m_RenderDataMap"))
            .FirstOrDefault(entry => RenderDataKeysMatch(entry.Get("first"), renderDataKey))
            ?.Get("second");
        if (spriteAtlasData is null || spriteAtlasData.IsDummy) return null;

        var texturePptr = spriteAtlasData.Get("texture");
        if (!context.Loader.TryResolvePPtr(atlasFileInstance, texturePptr, context.DependencyIndex, out var textureBaseField, out var textureFileInstance) ||
            textureBaseField is null || textureFileInstance is null)
            return null;

        return (textureFileInstance, textureBaseField, spriteAtlasData.Get("textureRect"));
    }

    private static bool RenderDataKeysMatch(AssetTypeValueField mapKey, AssetTypeValueField spriteKey)
    {
        var mapGuid = mapKey.Get("first");
        var spriteGuid = spriteKey.Get("first");
        for (var i = 0; i < 4; i++)
        {
            if (mapGuid.Get($"data[{i}]").AsUInt != spriteGuid.Get($"data[{i}]").AsUInt)
                return false;
        }

        return mapKey.Get("second").AsLong == spriteKey.Get("second").AsLong;
    }

    /// <summary>Reads+decodes a Texture2D field to top-down RGBA32 bytes (width*height*4). If
    /// <paramref name="fileInstance"/> is owned by a shared BundleDependencyIndex (i.e. it was
    /// reached through a cross-bundle hop, not this bundle's own private loader), FillPictureData
    /// reads raw bytes through that bundle's single shared reader - so the whole read+decode has to
    /// run under the index's lock, same as resolving the PPtr chain that got us here did.</summary>
    private static (byte[]? Pixels, int Width, int Height) DecodeTopDownRgba(
        AssetsFileInstance fileInstance, AssetTypeValueField textureBaseField, BundleDependencyIndex? dependencyIndex)
    {
        if (dependencyIndex is not null && dependencyIndex.Owns(fileInstance))
            return dependencyIndex.RunLocked(() => DecodeTopDownRgbaCore(fileInstance, textureBaseField));

        return DecodeTopDownRgbaCore(fileInstance, textureBaseField);
    }

    private static (byte[]? Pixels, int Width, int Height) DecodeTopDownRgbaCore(AssetsFileInstance fileInstance, AssetTypeValueField textureBaseField)
    {
        var texture = TextureFile.ReadTextureFile(textureBaseField);
        var textureData = texture.FillPictureData(fileInstance);
        if (textureData.Length == 0) return (null, 0, 0);

        var rgba = texture.DecodeTextureRaw(textureData, useBgra: false);
        if (rgba.Length == 0) return (null, 0, 0);

        // Unity stores rows bottom-to-top; PNG/SkiaSharp expect top-to-bottom.
        var stride = texture.m_Width * 4;
        var flipped = new byte[rgba.Length];
        for (var y = 0; y < texture.m_Height; y++)
            Buffer.BlockCopy(rgba, y * stride, flipped, (texture.m_Height - 1 - y) * stride, stride);

        return (flipped, texture.m_Width, texture.m_Height);
    }

    private static byte[] Crop(byte[] topDownRgba, int sourceWidth, int x, int y, int width, int height)
    {
        var result = new byte[width * height * 4];
        var srcStride = sourceWidth * 4;
        var dstStride = width * 4;
        for (var row = 0; row < height; row++)
        {
            var srcOffset = (y + row) * srcStride + x * 4;
            Buffer.BlockCopy(topDownRgba, srcOffset, result, row * dstStride, dstStride);
        }
        return result;
    }

    private static void WritePng(byte[] topDownRgba, int width, int height, string path)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        var handle = GCHandle.Alloc(topDownRgba, GCHandleType.Pinned);
        try
        {
            using var bitmap = new SKBitmap();
            bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), width * 4);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.Create(path);
            data.SaveTo(stream);
        }
        finally
        {
            handle.Free();
        }
    }
}
