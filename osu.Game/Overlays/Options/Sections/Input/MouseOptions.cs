// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Input
{
    public class MouseOptions : OptionsSubsection
    {
        protected override string Header => "Mouse";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionEnumDropdown<ConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor",
                    Bindable = config.GetBindable<ConfineMouseMode>(FrameworkConfig.ConfineMouseMode),
                },
                new OsuCheckbox
                {
                    LabelText = "Disable mouse wheel during gameplay",
                    Bindable = osuConfig.GetBindable<bool>(OsuConfig.MouseDisableWheel)
                },
                new OsuCheckbox
                {
                    LabelText = "Disable mouse buttons during gameplay",
                    Bindable = osuConfig.GetBindable<bool>(OsuConfig.MouseDisableButtons)
                },
            };
        }

        private class SensitivitySlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }
    }
}
