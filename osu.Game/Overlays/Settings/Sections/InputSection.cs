// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Overlays.Settings.Sections
{
    public class InputSection : SettingsSection
    {
        public override string Header => "Input";
        public override IconUsage Icon => FontAwesome.Regular.Keyboard;

        public InputSection(KeyBindingPanel keyConfig)
        {
            Children = new Drawable[]
            {
                new MouseSettings(),
                new KeyboardSettings(keyConfig),
            };
        }
    }
}
