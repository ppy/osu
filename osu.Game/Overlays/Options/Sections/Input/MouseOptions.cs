// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Input
{
    public class MouseOptions : OptionsSubsection
    {
        protected override string Header => "Mouse";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionSlider<double, SensitivitySlider>
                {
                    LabelText = "Sensitivity",
                    Bindable = config.GetBindable<double>(OsuConfig.MouseSpeed)
                },
                new OsuCheckbox
                {
                    LabelText = "Raw input",
                    Bindable = config.GetBindable<bool>(OsuConfig.RawInput),
                    TooltipText = "Raw input will bypass Windows acceleration and provide the most accurate mouse movement." //This should be less Windows specific
                },
                new OsuCheckbox
                {
                    LabelText = "Map absolute raw input to the osu! window",
                    Bindable = config.GetBindable<bool>(OsuConfig.AbsoluteToOsuWindow),
                    TooltipText = "Input devices with absolute positioning such as tablets usually affect the entire screen area. This allows your tablet screen area to be entirely dedicated to the osu! window."
                },
                new OptionEnumDropdown<ConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor",
                    Bindable = config.GetBindable<ConfineMouseMode>(OsuConfig.ConfineMouse),
                },
                new OsuCheckbox
                {
                    LabelText = "Disable mouse wheel in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableWheel),
                    TooltipText = "During play, you can use the mouse wheel to adjust the volume and pause the game. This will disable that functionality."
                },
                new OsuCheckbox
                {
                    LabelText = "Disable mouse buttons in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableButtons),
                    TooltipText = "This option will disable all mouse buttons. Specifically for people who use their keyboard to click."
                },
                new OsuCheckbox
                {
                    LabelText = "Cursor ripples",
                    Bindable = config.GetBindable<bool>(OsuConfig.CursorRipple),
                    TooltipText = "The cursor will ripple outwards on clicking." //Is this going to be implemented?!?
                },
            };
        }

        private class SensitivitySlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }
    }
}
