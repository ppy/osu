// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class InputSettings : PlayerSettingsGroup
    {
        private readonly PlayerCheckbox mouseButtonsCheckbox;

        public InputSettings()
            : base("Input Settings")
        {
            Children = new Drawable[]
            {
                mouseButtonsCheckbox = new PlayerCheckbox
                {
                    LabelText = MouseSettingsStrings.DisableMouseButtons
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config) => mouseButtonsCheckbox.Current = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
    }
}
