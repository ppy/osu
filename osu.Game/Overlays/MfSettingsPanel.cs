// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Platform.Linux;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Mf;

namespace osu.Game.Overlays
{
    public class MfSettingsPanel : SettingsSubPanel
    {
        protected override Drawable CreateHeader() => new SettingsHeader("Mf-osu自定义选项", "在这里调整Mf-osu的额外设置!");

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(GameHost host)
        {
            AddSection(new MfMainSection());

            if (host is LinuxGameHost)
            {
                AddSection(new LinuxSection());
            }

            AddSection(new MfMvisSection());
            AddSection(new MfMvisPluginSection());
            AddSection(new DangerousZone());
        }
    }
}
