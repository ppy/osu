// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MfMainSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Globe
        };

        public override string Header => "整体";

        public MfMainSection()
        {
            Add(new MfSettings());
        }
    }
}
