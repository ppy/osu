// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.DebugSettings
{
    public class MemorySettings : SettingsSubsection
    {
        protected override LocalisableString Header => DebugSettingsStrings.MemoryHeader;

        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config, GameHost host)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = DebugSettingsStrings.ClearAllCaches,
                    Action = host.Collect
                },
            };
        }
    }
}
