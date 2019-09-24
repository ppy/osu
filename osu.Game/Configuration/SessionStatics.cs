// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Configuration
{
    public class SessionStatics : InMemoryConfigManager<Statics>
    {
        protected override void InitialiseDefaults()
        {
            Set(Statics.LoginOverlayDisplayed, false);
        }
    }

    public enum Statics
    {
        LoginOverlayDisplayed,
    }
}
