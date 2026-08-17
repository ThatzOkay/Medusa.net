using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Server.Plugins;

public sealed record PluginUiEvent(string Type, string PluginUiId);

public sealed class PluginUiEventBroadcaster
{
    private readonly ConcurrentDictionary<string, Channel<PluginUiEvent>> _channels = new();

    public (string connectionId, Channel<PluginUiEvent> channel) Subscribe()
    {
        var id = Guid.NewGuid().ToString("N");
        var ch = Channel.CreateUnbounded<PluginUiEvent>(
            new UnboundedChannelOptions { SingleReader = true });
        _channels[id] = ch;
        return (id, ch);
    }

    public void Unsubscribe(string connectionId)
    {
        if (_channels.TryRemove(connectionId, out var ch))
            ch.Writer.TryComplete();
    }

    public void Broadcast(PluginUiEvent evt)
    {
        foreach (var ch in _channels.Values)
            ch.Writer.TryWrite(evt);
    }
}