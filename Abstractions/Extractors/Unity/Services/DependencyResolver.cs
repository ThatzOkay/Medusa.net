using System.Collections.Generic;
using Abstractions.Extractors.Unity.Helpers;
using Abstractions.Extractors.Unity.Models;
using AssetsTools.NET;

namespace Abstractions.Extractors.Unity.Services;

/// <summary>Walks an object's direct fields (and one level into any array fields) looking for
/// non-null PPtr references.</summary>
public static class DependencyResolver
{
    public static List<ObjectDependency> GetObjectDependencies(AssetTypeValueField baseField)
    {
        var dependencies = new List<ObjectDependency>();

        foreach (var child in baseField.Children)
        {
            if (ReflectionHelper.TryGetPPtr(child, out var pathId, out var fileId))
            {
                dependencies.Add(new ObjectDependency(child.FieldName, pathId, fileId));
                continue;
            }

            var i = 0;
            foreach (var element in BundleLoader.ArrayElements(child))
            {
                if (ReflectionHelper.TryGetPPtr(element, out var itemPathId, out var itemFileId))
                    dependencies.Add(new ObjectDependency($"{child.FieldName}[{i}]", itemPathId, itemFileId));
                i++;
            }
        }

        return dependencies;
    }
}
