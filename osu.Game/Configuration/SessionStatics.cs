// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        }
    }

    public enum Static
    {
        LoginOverlayDisplayed,
        MutedAudioNotificationShownOnce
    }
}
