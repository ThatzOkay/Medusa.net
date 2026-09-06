namespace Abstractions.Entities;

// Existence-range tracking for master data synced from successive game client dumps. A row's
// Id is its only primary key, so this is a single record per Id, not a per-client-version
// content snapshot - items only ever get added or dropped between versions, never revived, and
// their fields never change once inserted. Plugins that sync versioned master data (music,
// gacha, events, ...) should derive their entities from this and query them through
// IQueryable<T>.ForVersion(version) (see VersionedDataExtensions) rather than filtering
// MinVersion/MaxVersion by hand at each call site.
public class VersionedData
{
    public long InsertedVersion { get; set; }
    public long? MinVersion { get; set; }
    public long? MaxVersion { get; set; }
}
