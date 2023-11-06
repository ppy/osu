// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class InputSettings : PlayerSettingsGroup
    {
        private readonly PlayerCheckbox mouseButtonsCheckbox;

        public InputSettings()
            : base("Input Settings")
        {
            Children = new Drawable[]
            {
                mouseButtonsCheckbox = new PlayerCheckbox
                {
                    // TODO: change to touchscreen detection once https://github.com/ppy/osu/pull/25348 makes it in
                    LabelText = RuntimeInfo.IsDesktop ? MouseSettingsStrings.DisableClicksDuringGameplay : TouchSettingsStrings.DisableTapsDuringGameplay
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config) => mouseButtonsCheckbox.Current = config.GetBindable<bool>(RuntimeInfo.IsDesktop ? OsuSetting.MouseDisableButtons : OsuSetting.GameplayDisableTaps);
    }
}
