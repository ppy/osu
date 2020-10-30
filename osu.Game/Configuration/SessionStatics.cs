// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Configuration
{
    /// <summary>
    /// Stores global per-session statics. These will not be stored after exiting the game.
    /// </summary>
    public class SessionStatics : InMemoryConfigManager<Static>
    {
        protected override void InitialiseDefaults()
        {
            Set(Static.LoginOverlayDisplayed, false);
            Set(Static.MutedAudioNotificationShownOnce, false);
            Set<APISeasonalBackgrounds>(Static.SeasonalBackgroundsResponse, null);
        }
    }

    public enum Static
    {
        LoginOverlayDisplayed,
        MutedAudioNotificationShownOnce,
        SeasonalBackgroundsResponse,
    }
}
