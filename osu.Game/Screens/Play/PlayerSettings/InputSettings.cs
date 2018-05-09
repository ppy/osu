// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class InputSettings : PlayerSettingsGroup
    {
        protected override string Title => "Input settings";

        private readonly PlayerCheckbox mouseWheelCheckbox;
        private readonly PlayerCheckbox mouseButtonsCheckbox;

        public InputSettings()
        {
            Children = new Drawable[]
            {
                mouseWheelCheckbox = new PlayerCheckbox
                {
                    LabelText = "Disable mouse wheel"
                },
                mouseButtonsCheckbox = new PlayerCheckbox
                {
                    LabelText = "Disable mouse buttons"
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseWheelCheckbox.Bindable = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
            mouseButtonsCheckbox.Bindable = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
        }
    }
}
