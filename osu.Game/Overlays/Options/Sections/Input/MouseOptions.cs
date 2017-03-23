// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
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
                new OptionSlider<double>
                {
                    LabelText = "Sensitivity",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.MouseSpeed),
                },
                new OsuCheckbox
                {
                    LabelText = "Raw input",
                    Bindable = config.GetBindable<bool>(OsuConfig.RawInput)
                },
                new OsuCheckbox
                {
                    LabelText = "Map absolute raw input to the osu! window",
                    Bindable = config.GetBindable<bool>(OsuConfig.AbsoluteToOsuWindow)
                },
                new OptionEnumDropdown<ConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor",
                    Bindable = config.GetBindable<ConfineMouseMode>(OsuConfig.ConfineMouse),
                },
                new OsuCheckbox
                {
                    LabelText = "Disable mouse wheel in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableWheel)
                },
                new OsuCheckbox
                {
                    LabelText = "Disable mouse buttons in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableButtons)
                },
                new OsuCheckbox
                {
                    LabelText = "Cursor ripples",
                    Bindable = config.GetBindable<bool>(OsuConfig.CursorRipple)
                },
            };
        }
    }
}
