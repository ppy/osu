// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class KeyboardSettings : SettingsSubsection
    {
        protected override string Header => "Keyboard";

        public KeyboardSettings(KeyBindingOverlay keyConfig)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "Key Configuration",
                    Action = keyConfig.ToggleVisibility
                },
            };
        }
    }
}
