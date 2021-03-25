// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class BindingSettings : SettingsSubsection
    {
        protected override string Header => "Shortcut and gameplay bindings";

        public BindingSettings(KeyBindingPanel keyConfig)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "Configure",
                    TooltipText = "change global shortcut keys and gameplay bindings",
                    Action = keyConfig.ToggleVisibility
                },
            };
        }
    }
}
