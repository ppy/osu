// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;

namespace osu.Game.Configuration
{
    public class SessionStatics : ConfigManager<Statics>
    {
        // This is an in-memory store.
        protected override void PerformLoad()
        {
        }

        protected override bool PerformSave() => true;
    }

    public enum Statics
    {
        LoginOverlayDisplayed,
    }
}
