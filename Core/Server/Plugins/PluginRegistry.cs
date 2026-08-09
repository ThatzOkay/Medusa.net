using System.Collections.Immutable;
using Abstractions;

namespace Server.Plugins;

public sealed class PluginRegistry
{
    private ImmutableDictionary<PluginSlotKey, PluginSlot> _slots = ImmutableDictionary<PluginSlotKey, PluginSlot>.Empty;

    internal void Add(PluginSlot slot)
    {
        ImmutableInterlocked.AddOrUpdate(ref _slots, slot.Key, slot, (_, _) => slot);
    }

    internal PluginSlot? Remove(PluginSlotKey key)
    {
        _slots.TryGetValue(key, out var old);
        ImmutableInterlocked.TryRemove(ref _slots, key, out _);
        return old;
    }
    
    public PluginSlot? GetSlot(PluginSlotKey key)
    {
        _slots.TryGetValue(key, out var slot);
        return slot;
    }
    
    public IEnumerable<PluginSlot> GetSlots() => _slots.Values;
    public IEnumerable<IMedusaPlugin> GetPlugins() => _slots.Values.Select(s => s.Plugin);
}

public sealed record PluginSlotKey(string GameCode, int? MinVer, int? MaxVer);