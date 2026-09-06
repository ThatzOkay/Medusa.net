using System.Linq;
using Abstractions.Entities;

namespace Abstractions.Utils;

public static class VersionedDataExtensions
{
    // A row is visible to a client on `version` while MinVersion <= version <= MaxVersion,
    // with either bound null meaning unbounded on that side (MinVersion is set once at insert
    // and never changes; MaxVersion stays null while the row is still present in the latest
    // synced client dump, and gets stamped with the version it was dropped in once it
    // disappears from a later dump).
    public static IQueryable<T> ForVersion<T>(this IQueryable<T> query, long version) where T : VersionedData =>
        query.Where(e => (e.MinVersion == null || e.MinVersion <= version)
                       && (e.MaxVersion == null || e.MaxVersion >= version));
}
