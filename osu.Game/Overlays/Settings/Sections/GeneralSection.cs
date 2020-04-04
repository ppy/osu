// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.General;

namespace osu.Game.Overlays.Settings.Sections
{
    public class GeneralSection : SettingsSection
    {
        public override string Header => "General";
        public override IconUsage Icon => FontAwesome.Solid.Cog;

        public GeneralSection()
        {
            Children = new Drawable[]
            {
                new LanguageSettings(),
                new UpdateSettings(),
            };
        }
    }
}
