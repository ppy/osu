// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.UserInterface;

namespace osu.Game.Overlays.Settings.Sections
{
    public class UserInterfaceSection : SettingsSection
    {
        public override string Header => "User Interface";

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.LayerGroup
        };

        public UserInterfaceSection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new MainMenuSettings(),
                new SongSelectSettings()
            };
        }
    }
}
