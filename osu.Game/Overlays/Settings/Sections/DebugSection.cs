// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.DebugSettings;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class DebugSection : SettingsSection
    {
        public override LocalisableString Header => @"Debug";

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = OsuIcon.Debug
        };

        public DebugSection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new BatchImportSettings(),
                new MemorySettings(),
            };
        }
    }
}
