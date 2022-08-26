// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class BindingSettings : SettingsSubsection
    {
        protected override LocalisableString Header => BindingSettingsStrings.ShortcutAndGameplayBindings;

        public BindingSettings(KeyBindingPanel keyConfig)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = BindingSettingsStrings.Configure,
                    TooltipText = BindingSettingsStrings.ChangeBindingsButton,
                    Action = keyConfig.ToggleVisibility
                },
            };
        }
    }
}
