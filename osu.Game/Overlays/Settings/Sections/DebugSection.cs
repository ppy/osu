// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.DebugSettings;

namespace osu.Game.Overlays.Settings.Sections
{
    public class DebugSection : SettingsSection
    {
        public override LocalisableString Header => DebugSettingsStrings.DebugSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Bug
        };

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
