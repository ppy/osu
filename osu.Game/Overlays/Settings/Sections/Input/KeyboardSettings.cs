// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
                    Text = "Key configuration",
                    Action = keyConfig.ToggleVisibility
                },
            };
        }
    }
}
