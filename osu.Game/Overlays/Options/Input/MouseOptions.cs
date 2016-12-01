using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Input
{
    public class MouseOptions : OptionsSubsection
    {
        protected override string Header => "Mouse";

        private CheckBoxOption rawInput, mapRawInput, disableWheel, disableButtons, enableRipples;

        public MouseOptions()
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Sensitivity: TODO slider" },
                new OptionsSlider<double>
                {
                    Label = "Sensitivity",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.MouseSpeed),
                },
                rawInput = new CheckBoxOption
                {
                    LabelText = "Raw input",
                    Bindable = config.GetBindable<bool>(OsuConfig.RawInput)
                },
                mapRawInput = new CheckBoxOption
                {
                    LabelText = "Map absolute raw input to the osu! window",
                    Bindable = config.GetBindable<bool>(OsuConfig.AbsoluteToOsuWindow)
                },
                new SpriteText { Text = "Confine mouse cursor: TODO dropdown" },
                disableWheel = new CheckBoxOption
                {
                    LabelText = "Disable mouse wheel in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableWheel)
                },
                disableButtons = new CheckBoxOption
                {
                    LabelText = "Disable mouse buttons in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableButtons)
                },
                enableRipples = new CheckBoxOption
                {
                    LabelText = "Cursor ripples",
                    Bindable = config.GetBindable<bool>(OsuConfig.CursorRipple)
                },
            };
        }
    }
}
