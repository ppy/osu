// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Overlays.Settings.Sections
{
    public class InputSection : SettingsSection
    {
        public override string Header => "Input";
        public override FontAwesome Icon => FontAwesome.fa_keyboard_o;

        public InputSection(KeyBindingOverlay keyConfig)
        {
            Children = new Drawable[]
            {
                new MouseSettings(),
                new KeyboardSettings(keyConfig),
            };
        }
    }
}
