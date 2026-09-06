using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Abstractions.Extractors.IFS;

/// <summary>Extracts and replaces textures inside AVS/IFS archives.</summary>
public static class IfsFile
{
    private const uint Magic = 0x6CAD8F89;

    private static byte[] Concat(params byte[][] parts)
    {
        var result = new byte[parts.Sum(part => part.Length)];
        var offset = 0;
        foreach (var part in parts)
        {
            part.CopyTo(result, offset);
            offset += part.Length;
        }
        return result;
    }

    private static List<KNode> ValidateIntegrity(byte[] input, byte[] manifestData, KNode manifest, int manifestEnd)
    {
        var manifestHash = MD5.HashData(manifestData);
        if (!input.AsSpan(20, 16).SequenceEqual(manifestHash))
            throw new InvalidDataException("IFS manifest checksum is invalid.");

        var info = KBin.FindChild(manifest, "_info_");
        var dataSizeNode = info is not null ? KBin.FindChild(info, "size") : null;
        var dataHashNode = info is not null ? KBin.FindChild(info, "md5") : null;
        if (dataSizeNode is null || dataHashNode is null || dataHashNode.Values.Length != 16)
            throw new InvalidDataException("IFS integrity fields were not found.");

        var data = input.AsSpan(manifestEnd).ToArray();
        if ((int)dataSizeNode.Values[0] != data.Length)
            throw new InvalidDataException("IFS data size is invalid.");
        var expectedDataHash = dataHashNode.Values.Select(v => (byte)v).ToArray();
        if (!MD5.HashData(data).AsSpan().SequenceEqual(expectedDataHash))
            throw new InvalidDataException("IFS data checksum is invalid.");

        var fileNodes = KBin.Walk(manifest).Where(node => node.Values.Length == 3 && node.ValueOffset is not null).ToList();
        foreach (var node in fileNodes)
        {
            var offset = node.Values[0];
            var size = node.Values[1];
            if (offset != Math.Floor(offset) || size != Math.Floor(size) || offset < 0 || size < 0 || offset + size > data.Length)
                throw new InvalidDataException($"IFS entry {KBin.FixedName(node.Name)} has invalid bounds.");
        }
        return fileNodes;
    }

    private static void WriteNodeNumber(byte[] buffer, KNode node, int index, int value)
    {
        if (!KBin.Formats.TryGetValue(node.Type, out var format) || node.ValueOffset is null || format.Float)
            throw new InvalidDataException($"Unsupported IFS manifest value {node.Name}");
        var offset = node.ValueOffset.Value + index * format.Size;
        if (format.Size == 1)
            buffer[offset] = unchecked((byte)value);
        else if (format.Size == 2)
        {
            if (format.Signed) BinaryPrimitives.WriteInt16BigEndian(buffer.AsSpan(offset, 2), (short)value);
            else BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), (ushort)value);
        }
        else if (format.Size == 4)
        {
            if (format.Signed) BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset, 4), value);
            else BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(offset, 4), (uint)value);
        }
        else
            throw new InvalidDataException($"Unsupported IFS manifest number size {format.Size}");
        node.Values[index] = value;
    }

    private static string HashName(string imageName) =>
        Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(imageName)));

    private static string BuildRelativePath(KNode manifest, KNode node)
    {
        var segments = new List<string>();
        for (var current = node; current is not null && current != manifest; current = current.Parent)
            segments.Add(KBin.FixedName(current.Name));
        segments.Reverse();
        return Path.Combine(segments.ToArray());
    }

    private static bool MatchesFilter(string? pattern, string candidate)
    {
        if (string.IsNullOrEmpty(pattern)) return true;
        var normalized = candidate.Replace('\\', '/');
        return FileSystemName.MatchesSimpleExpression(pattern, Path.GetFileName(normalized), ignoreCase: true)
            || FileSystemName.MatchesSimpleExpression(pattern, normalized, ignoreCase: true);
    }

    /// <summary>Where <see cref="ExtractAll"/> writes a given archive's output, so callers can check it up front (e.g. to skip already-extracted archives).</summary>
    public static string GetOutputDirectory(string source, string outputRoot) =>
        Path.Combine(outputRoot, $"{Path.GetFileNameWithoutExtension(source)}_ifs");

    /// <summary>
    /// Extracts every texture from <paramref name="source"/> as decoded PNGs, and dumps every other raw
    /// entry in the archive (audio, AFP/BSI sprite metadata, or anything else the manifest describes) verbatim,
    /// all into <c>&lt;outputRoot&gt;/&lt;name&gt;_ifs/</c>, mirroring the archive's own folder structure.
    /// </summary>
    /// <remarks>
    /// Patch archives (those referencing a base archive via <c>_super_</c>) are not supported and will
    /// cause the extraction to be refused rather than risk extracting incorrect data.
    /// </remarks>
    /// <param name="source">Path to the IFS archive file to extract.</param>
    /// <param name="outputRoot">Root directory where the <c>&lt;name&gt;_ifs/</c> output folder will be created.</param>
    /// <param name="namePattern">Only extract entries whose name matches this glob (e.g. "gr*"); null extracts everything.</param>
    /// <param name="includeTextures">Whether to decode and write texture images as PNGs.</param>
    /// <param name="includeRaw">Whether to dump every other entry (audio, sprite metadata, etc.) as a raw file.</param>
    /// <returns>A read-only list of full paths to every file that was written.</returns>
    public static IReadOnlyList<string> ExtractAll(string source, string outputRoot,
        string? namePattern = null, bool includeTextures = true, bool includeRaw = true)
    {
        var input = File.ReadAllBytes(source);
        if (input.Length < 36 || BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(0, 4)) != Magic || BinaryPrimitives.ReadUInt16BigEndian(input.AsSpan(4, 2)) < 2)
            throw new InvalidDataException("Unsupported or invalid IFS file");
        var manifestEnd = (int)BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(16, 4));
        if (manifestEnd <= 36 || manifestEnd > input.Length) throw new InvalidDataException("Invalid IFS manifest size");
        var manifestData = input.AsSpan(36, manifestEnd - 36).ToArray();
        var manifest = KBin.Read(manifestData);
        ValidateIntegrity(input, manifestData, manifest, manifestEnd);

        var outputDirectory = GetOutputDirectory(source, outputRoot);
        Directory.CreateDirectory(outputDirectory);
        var outputDirectoryFull = Path.GetFullPath(outputDirectory);

        var outputs = new List<string>();
        var consumedOffsets = new HashSet<int>();

        var tex = KBin.FindChild(manifest, "tex");
        if (tex is not null)
        {
            var files = new Dictionary<string, (int Offset, int Size)>();
            foreach (var entry in tex.Children)
                if (entry.Values.Length >= 2)
                    files[KBin.FixedName(entry.Name)] = ((int)entry.Values[0], (int)entry.Values[1]);

            var textureListName = files.Keys.FirstOrDefault(name => name.EndsWith(".xml", StringComparison.Ordinal));
            if (textureListName is null) throw new InvalidDataException("IFS texture list was not found");
            var (xmlOffset, xmlSize) = files[textureListName];
            consumedOffsets.Add(xmlOffset);
            var textureListData = input.AsSpan(manifestEnd + xmlOffset, xmlSize).ToArray();
            var textureList = KBin.Read(textureListData);
            var compress = textureList.Attrs.GetValueOrDefault("compress", "");

            foreach (var texture in textureList.Children)
            {
                var format = texture.Attrs.GetValueOrDefault("format");
                foreach (var image in texture.Children.Where(node => node.Name == "image"))
                {
                    var imageName = image.Attrs.GetValueOrDefault("name");
                    var imgrectNode = KBin.FindChild(image, "imgrect");
                    if (string.IsNullOrEmpty(imageName) || imgrectNode is null || imgrectNode.Values.Length < 4) continue;

                    var manifestName = HashName(imageName);
                    if (!files.TryGetValue(manifestName, out var entry) && !files.TryGetValue($"_{manifestName}", out entry)) continue;
                    // Mark this offset as belonging to the texture pipeline regardless of includeTextures/namePattern,
                    // so a skipped-but-identified texture never falls through into the generic raw dump below.
                    consumedOffsets.Add(entry.Offset);
                    if (!includeTextures || !MatchesFilter(namePattern, imageName)) continue;

                    if (imageName.Contains('/') || imageName.Contains('\\') || imageName.Contains('\0'))
                        throw new InvalidDataException("IFS texture name contains an invalid path.");

                    var data = input.AsSpan(manifestEnd + entry.Offset, entry.Size).ToArray();
                    if (compress == "avslz")
                    {
                        if (data.Length < 8) throw new InvalidDataException($"Invalid AVSLZ texture {imageName}");
                        var uncompressed = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(0, 4));
                        var compressedLength = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(4, 4));
                        if (uncompressed > 64 * 1024 * 1024) throw new InvalidDataException($"AVSLZ texture {imageName} is too large.");
                        if (data.Length == compressedLength + 8)
                        {
                            data = Avslz.Decompress(data[8..]);
                            if (data.Length != uncompressed) throw new InvalidDataException($"AVSLZ size mismatch for {imageName}");
                        }
                        else
                        {
                            data = Concat(data[8..], data[..8]);
                        }
                    }

                    var width = (int)Math.Floor((imgrectNode.Values[1] - imgrectNode.Values[0]) / 2);
                    var height = (int)Math.Floor((imgrectNode.Values[3] - imgrectNode.Values[2]) / 2);
                    if (width <= 0 || height <= 0 || width > 8192 || height > 8192 || (long)width * height > 16 * 1024 * 1024)
                        throw new InvalidDataException($"IFS texture {imageName} has invalid dimensions.");

                    var output = Path.GetFullPath(Path.Combine(outputDirectory, $"{imageName}.png"));
                    if (!output.StartsWith(outputDirectoryFull + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                        throw new InvalidDataException("IFS texture output path is invalid.");

                    File.WriteAllBytes(output, Png.Encode(width, height, PixelCodec.Decode(format, data, width, height)));
                    outputs.Add(output);
                }
            }
        }

        // Dump every remaining raw entry the manifest describes -- audio (.2dx/.bin), AFP/BSI sprite
        // metadata, unrecognized image entries, and anything else -- so nothing is silently skipped.
        if (includeRaw)
        {
            foreach (var node in KBin.Walk(manifest).Where(n => n.Values.Length == 3 && n.ValueOffset is not null))
            {
                var offset = (int)node.Values[0];
                var size = (int)node.Values[1];
                if (consumedOffsets.Contains(offset)) continue;
                if (offset < 0 || size < 0 || offset + size > input.Length - manifestEnd) continue;

                var relativePath = BuildRelativePath(manifest, node);
                if (string.IsNullOrEmpty(relativePath) || !MatchesFilter(namePattern, relativePath)) continue;
                var output = Path.GetFullPath(Path.Combine(outputDirectory, relativePath));
                if (!output.StartsWith(outputDirectoryFull + Path.DirectorySeparatorChar, StringComparison.Ordinal)) continue;

                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                File.WriteAllBytes(output, input.AsSpan(manifestEnd + offset, size).ToArray());
                outputs.Add(output);
            }
        }

        return outputs;
    }

    /// <summary>Replaces one texture's pixels in <paramref name="source"/>, writing the patched archive to <paramref name="destination"/>. Returns a re-encoded PNG preview of the pixels actually written.</summary>
    public static byte[] ReplaceTexture(string source, string destination, string imageName, int width, int height, byte[] png)
    {
        var sourcePath = Path.GetFullPath(source);
        var destinationPath = Path.GetFullPath(destination);
        var sameFile = OperatingSystem.IsWindows()
            ? string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase)
            : sourcePath == destinationPath;
        if (sameFile) throw new InvalidOperationException("Refusing to overwrite the source IFS directly.");

        var (_, _, rgba) = Png.Decode(png, width, height);
        var input = File.ReadAllBytes(source);
        if (input.Length < 36 || BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(0, 4)) != Magic || BinaryPrimitives.ReadUInt16BigEndian(input.AsSpan(4, 2)) < 2)
            throw new InvalidDataException("Unsupported or invalid IFS file.");
        var manifestEnd = (int)BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(16, 4));
        if (manifestEnd <= 36 || manifestEnd > input.Length) throw new InvalidDataException("Invalid IFS manifest size.");
        var manifestData = input.AsSpan(36, manifestEnd - 36).ToArray();
        var manifest = KBin.Read(manifestData);
        var fileNodes = ValidateIntegrity(input, manifestData, manifest, manifestEnd);

        var tex = KBin.FindChild(manifest, "tex");
        if (tex is null) throw new InvalidDataException("IFS texture folder was not found.");

        var files = new Dictionary<string, KNode>();
        foreach (var node in tex.Children)
            if (node.Values.Length >= 2)
                files[KBin.FixedName(node.Name)] = node;

        var textureListName = files.Keys.FirstOrDefault(name => name.EndsWith(".xml", StringComparison.Ordinal));
        if (textureListName is null) throw new InvalidDataException("IFS texture list was not found.");
        var textureListNode = files[textureListName];
        var textureListOffset = (int)textureListNode.Values[0];
        var textureListSize = (int)textureListNode.Values[1];
        var textureList = KBin.Read([.. input.AsSpan(manifestEnd + textureListOffset, textureListSize)]);

        var format = "";
        var compress = textureList.Attrs.GetValueOrDefault("compress", "");
        int actualWidth = 0, actualHeight = 0;
        foreach (var texture in textureList.Children)
        {
            var image = texture.Children.FirstOrDefault(node => node.Name == "image" && node.Attrs.GetValueOrDefault("name") == imageName);
            if (image is null) continue;
            var imgrectNode = KBin.FindChild(image, "imgrect");
            if (imgrectNode is null || imgrectNode.Values.Length < 4) throw new InvalidDataException("IFS texture dimensions were not found.");
            format = texture.Attrs.GetValueOrDefault("format", "");
            actualWidth = (int)Math.Floor((imgrectNode.Values[1] - imgrectNode.Values[0]) / 2);
            actualHeight = (int)Math.Floor((imgrectNode.Values[3] - imgrectNode.Values[2]) / 2);
            break;
        }
        if (actualWidth != width || actualHeight != height)
            throw new InvalidDataException($"IFS texture size is {actualWidth} x {actualHeight}, not {width} x {height}.");
        if (format != "argb8888rev")
            throw new InvalidDataException($"Unsupported writable IFS texture format {(string.IsNullOrEmpty(format) ? "unknown" : format)}.");

        var packedName = HashName(imageName);
        if (!files.TryGetValue(packedName, out var targetNode) && !files.TryGetValue($"_{packedName}", out targetNode))
            throw new InvalidDataException($"IFS texture {imageName} was not found.");
        var targetOffset = (int)targetNode.Values[0];
        var targetSize = (int)targetNode.Values[1];

        var raw = PixelCodec.EncodeArgb8888Rev(rgba);
        var packed = raw;
        if (compress == "avslz")
        {
            var compressedLiterals = Avslz.CompressLiterals(raw);
            var header = new byte[8];
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), (uint)raw.Length);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), (uint)compressedLiterals.Length);
            packed = Concat(header, compressedLiterals);
        }
        else if (!string.IsNullOrEmpty(compress))
        {
            throw new InvalidDataException($"Unsupported IFS texture compression {compress}.");
        }

        var originalData = input.AsSpan(manifestEnd).ToArray();
        var nextOffset = fileNodes
            .Select(node => (int)node.Values[0])
            .Where(offset => offset > targetOffset)
            .DefaultIfEmpty(KBin.Align16(targetOffset + targetSize))
            .Min();
        if (targetOffset < 0 || targetSize < 0 || nextOffset < targetOffset + targetSize || nextOffset > originalData.Length)
            throw new InvalidDataException("IFS texture entry has invalid bounds.");

        var padding = new byte[KBin.Align16(packed.Length) - packed.Length];
        var replacement = Concat(packed, padding);
        var oldSpan = nextOffset - targetOffset;
        var delta = replacement.Length - oldSpan;
        var data = Concat(originalData[..targetOffset], replacement, originalData[nextOffset..]);

        WriteNodeNumber(manifestData, targetNode, 1, packed.Length);
        foreach (var node in fileNodes)
        {
            var offset = (int)node.Values[0];
            if (node != targetNode && offset >= nextOffset) WriteNodeNumber(manifestData, node, 0, offset + delta);
        }

        var info = KBin.FindChild(manifest, "_info_");
        var dataSizeNode = info is not null ? KBin.FindChild(info, "size") : null;
        var dataHashNode = info is not null ? KBin.FindChild(info, "md5") : null;
        if (dataSizeNode is null || dataHashNode is null || dataHashNode.ValueOffset is null || dataHashNode.Values.Length != 16)
            throw new InvalidDataException("IFS integrity fields were not found.");
        WriteNodeNumber(manifestData, dataSizeNode, 0, data.Length);
        MD5.HashData(data).CopyTo(manifestData.AsSpan(dataHashNode.ValueOffset.Value));

        var header2 = input.AsSpan(0, 36).ToArray();
        MD5.HashData(manifestData).CopyTo(header2.AsSpan(20));
        File.WriteAllBytes(destination, Concat(header2, manifestData, data));
        return Png.Encode(width, height, rgba);
    }
}
