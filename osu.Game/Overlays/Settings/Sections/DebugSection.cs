// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.Debug;

namespace osu.Game.Overlays.Settings.Sections
{
    public class DebugSection : SettingsSection
    {
        public override string Header => "Debug";
        public override IconUsage Icon => FontAwesome.Solid.Bug;

        public DebugSection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new MemorySettings(),
            };
        }
    }
}
