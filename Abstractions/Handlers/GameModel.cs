using System.Linq;

namespace Abstractions.Handlers;

// Every /eamuse request carries its `model` as "model:dest:spec:rev:ext" (e.g.
// "XIF:J:A:A:2025070800" - confirmed against real captured traffic AND against a real cabinet's
// own prop/ea3-ident.xml, which names these same five fields <model>/<dest>/<spec>/<rev>/<ext>).
// GameCode/Version below are named for what the rest of this codebase already calls them
// (HandlerGameCode, IMedusaPlugin.GameCode, and every handler's version-gating) rather than
// the raw wire names "model"/"ext" - GameCode is model, Version is ext.
//
// Parsed once here instead of each handler doing its own model.Split(...): a wrong separator or
// index there doesn't fail loudly, it just silently mis-parses (or throws FormatException on
// every real request while looking fine against a hand-built test value).
public readonly record struct GameModel(string GameCode, string Dest, string Spec, string Rev, long Version)
{
    public static GameModel Parse(string model)
    {
        var parts = model.Split(':');

        var version = parts.Length > 4 ? string.Join(string.Empty, parts.Skip(4)) : "";

        return new GameModel(
            parts.Length > 0 ? parts[0] : "",
            parts.Length > 1 ? parts[1] : "",
            parts.Length > 2 ? parts[2] : "",
            parts.Length > 3 ? parts[3] : "",
            string.IsNullOrEmpty(version) ? 0 : long.Parse(version));
    }

    public override string ToString() => $"{GameCode}:{Dest}:{Spec}:{Rev}:{Version}";
}
