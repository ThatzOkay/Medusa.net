namespace Abstractions.Extractors.Unity.Models;

/// <summary>Per-object type/location metadata, written to type_tree.json.</summary>
public record TypeTreeInfo
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = "";
    public long PathId { get; set; }
    public long DataOffset { get; set; }
    public long DataSize { get; set; }
    public int ScriptTypeIndex { get; set; } = -1;
    public string Hash { get; set; } = "";
    public string[] TypeDependencies { get; set; } = [];
}