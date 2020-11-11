// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;

namespace osu.Game.Overlays.Settings.Sections.Debug
{
    public class MemorySettings : SettingsSubsection
    {
        protected override string Header => "Memory";

        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config, GameHost host)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "Clear all caches",
                    Action = host.Collect
                },
            };
        }
    }
}
