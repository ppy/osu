// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.General;

namespace osu.Game.Overlays
{
    public class MfMainSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Globe
        };
        public override string Header => "界面选项";

        public MfMainSection()
        {
            Add(new MfSettings());
            Add(new MvisSettings());
        }
    }
}
