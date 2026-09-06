namespace Abstractions.Extractors.Unity.Models;

/// <summary>A single outgoing PPtr reference found while walking an object's fields. Mirrors get_object_dependencies().</summary>
public record ObjectDependency(string Attribute, long PathId, int FileId);