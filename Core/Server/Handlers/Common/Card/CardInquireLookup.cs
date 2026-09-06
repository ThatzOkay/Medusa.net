using Abstractions;
using Abstractions.Entities;
using Abstractions.Handlers;
using Abstractions.Services;
using Server.Services;

namespace Server.Handlers.Common.Card;

/// <summary>
/// Result of resolving a card for an `inquire`/`gninquire`-style request.
/// When <see cref="ErrorStatus"/> is set, the other fields are unpopulated and the
/// caller should return an early-out response carrying that status.
/// </summary>
internal sealed record CardInquireLookup(
    int? ErrorStatus,
    string KonamiId,
    Abstractions.Entities.Card? ExistingCard,
    IMedusaPlugin? Plugin,
    bool ProfileExists)
{
    /// <summary>
    /// Shared validation/lookup chain used by both InquireCardManagementHandler and
    /// GnInquireCardManagementHandler: resolves the plugin for the model's game code,
    /// converts the card UID to a Konami ID, and checks whether a profile exists for it.
    /// </summary>
    public static async Task<CardInquireLookup> ResolveAsync(string? cardId, GameModel model,
        IPluginService pluginService, ICardService cardService)
    {
        var gameCode = model.GameCode;
        var plugin = pluginService.FindPlugin(gameCode);

        if (string.IsNullOrEmpty(cardId))
        {
            return new CardInquireLookup(111, "", null, null, false);
        }

        var konamiId = cardService.ConvertUidToKonamiId(cardId);

        if (string.IsNullOrEmpty(konamiId))
        {
            return new CardInquireLookup(111, "", null, null, false);
        }

        var existingCard = await cardService.FindByKonamiId(konamiId);

        if (existingCard is null)
        {
            return new CardInquireLookup(112, konamiId, null, null, false);
        }

        if (plugin is null)
        {
            return new CardInquireLookup(113, konamiId, existingCard, null, false);
        }

        var profileExists = await pluginService.DoesProfileExistAsync(plugin, cardId);

        return new CardInquireLookup(null, konamiId, existingCard, plugin, profileExists);
    }
}