// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;

namespace osu.Game.Overlays.Mods
{
    public class ModSelectOverlayStatics : InMemoryConfigManager<Static>
    {
        protected override void InitialiseDefaults()
        {
            SetDefault(Static.LastModSelectPanelSoundPlaybackTime, (double?)null);
        }
    }

    public enum Static
    {
        LastModSelectPanelSoundPlaybackTime
    }
}
