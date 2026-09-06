using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Services;
using AssetsTools.NET;

namespace Abstractions.Extractors.Unity;

/// <summary>
/// Exports mesh geometry to an .obj file. AssetsTools.NET's raw typetree doesn't decode Unity's
/// compressed GPU vertex buffer format, so this only writes real geometry for meshes old enough to
/// still carry flat m_Vertices/m_Triangles fields. Anything using the modern packed m_VertexData
/// layout falls through to a plain typetree dump instead.
/// </summary>
public sealed class MeshExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["Mesh"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseField = context.BaseField;
            var verticesField = baseField.Get("m_Vertices");
            var trianglesField = baseField.Get("m_Triangles");

            var vertices = verticesField.IsDummy ? [] : ReadVector3Array(verticesField);
            var triangles = trianglesField.IsDummy ? [] : BundleLoader.ArrayElements(trianglesField).Select(f => f.AsUInt).ToList();

            var meshData = new Dictionary<string, object?>
            {
                ["name"] = baseField.Get("m_Name") is { IsDummy: false } n ? n.AsString : "Unknown",
                ["vertex_count"] = vertices.Count,
                ["triangle_count"] = triangles.Count / 3,
                ["submesh_count"] = BundleLoader.ArrayElements(baseField.Get("m_SubMeshes")).Count(),
                ["blend_shapes"] = BundleLoader.ArrayElements(baseField.Get("m_Shapes").Get("shapes")).Count(),
                ["bone_count"] = BundleLoader.ArrayElements(baseField.Get("m_BoneNameHashes")).Count(),
            };

            if (vertices.Count > 0)
            {
                var obj = new StringBuilder();
                obj.AppendLine($"# Mesh: {meshData["name"]}");
                obj.AppendLine($"# Vertices: {vertices.Count}");
                foreach (var v in vertices)
                    obj.AppendLine(FormattableString.Invariant($"v {v.X:F6} {v.Y:F6} {v.Z:F6}"));

                for (var i = 0; i + 2 < triangles.Count; i += 3)
                    obj.AppendLine($"f {triangles[i] + 1} {triangles[i + 1] + 1} {triangles[i + 2] + 1}");

                await ObjectSerializer.WriteTextAsync(obj.ToString(), context.OutputPathNoExtension + ".obj", cancellationToken);
            }

            await ObjectSerializer.WriteJsonAsync(meshData, context.OutputPathNoExtension + "_info.json", cancellationToken);
            return true;
        }
        catch
        {
            return await FallbackExtractor.WriteTypeTreeDumpAsync(context, cancellationToken);
        }
    }

    private static List<(float X, float Y, float Z)> ReadVector3Array(AssetTypeValueField verticesField)
    {
        var result = new List<(float, float, float)>();
        foreach (var v in BundleLoader.ArrayElements(verticesField))
        {
            var x = v.Get("x") is { IsDummy: false } xf ? xf.AsFloat : 0f;
            var y = v.Get("y") is { IsDummy: false } yf ? yf.AsFloat : 0f;
            var z = v.Get("z") is { IsDummy: false } zf ? zf.AsFloat : 0f;
            result.Add((x, y, z));
        }
        return result;
    }
}
