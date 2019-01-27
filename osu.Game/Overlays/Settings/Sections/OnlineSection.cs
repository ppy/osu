// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Online;

namespace osu.Game.Overlays.Settings.Sections
{
    public class OnlineSection : SettingsSection
    {
        public override string Header => "Online";
        public override FontAwesome Icon => FontAwesome.fa_globe;

        public OnlineSection()
        {
            Children = new Drawable[]
            {
                new WebSettings()
            };
        }
    }
}
