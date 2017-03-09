// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;

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
                new OptionSlider<double>
                {
                    LabelText = "Sensitivity",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.MouseSpeed),
                },
                new OptionCheckbox
                {
                    LabelText = "Raw input",
                    Bindable = config.GetBindable<bool>(OsuConfig.RawInput)
                },
                new OptionCheckbox
                {
                    LabelText = "Map absolute raw input to the osu! window",
                    Bindable = config.GetBindable<bool>(OsuConfig.AbsoluteToOsuWindow)
                },
                new OptionEnumDropDown<ConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor",
                    Bindable = config.GetBindable<ConfineMouseMode>(OsuConfig.ConfineMouse),
                },
                new OptionCheckbox
                {
                    LabelText = "Disable mouse wheel in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableWheel)
                },
                new OptionCheckbox
                {
                    LabelText = "Disable mouse buttons in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableButtons)
                },
                new OptionCheckbox
                {
                    LabelText = "Cursor ripples",
                    Bindable = config.GetBindable<bool>(OsuConfig.CursorRipple)
                },
            };
        }
    }
}
