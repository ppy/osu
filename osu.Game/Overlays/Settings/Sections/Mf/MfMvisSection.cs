// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.General;

namespace osu.Game.Overlays
{
    public class MfMvisSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Regular.PlayCircle
        };
        public override string Header => "M-vis播放器";

        public MfMvisSection()
        {
            Add(new MvisUISettings());
            Add(new MvisStoryBoardSettings());
            Add(new MvisVisualSettings());
        }
    }
}
