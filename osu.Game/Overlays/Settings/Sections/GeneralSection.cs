// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.General;

namespace osu.Game.Overlays.Settings.Sections
{
    public class GeneralSection : SettingsSection
    {
        public override string Header => "General";
        public override FontAwesome Icon => FontAwesome.fa_gear;

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
