using osu.Framework;
using osu.Framework.Allocation;
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

        [Initializer]
        private void Load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Sensitivity: TODO slider" },
                new CheckBoxOption
                {
                    LabelText = "Raw input",
                    Bindable = config.GetBindable<bool>(OsuConfig.RawInput)
                },
                new CheckBoxOption
                {
                    LabelText = "Map absolute raw input to the osu! window",
                    Bindable = config.GetBindable<bool>(OsuConfig.AbsoluteToOsuWindow)
                },
                new SpriteText { Text = "Confine mouse cursor: TODO dropdown" },
                new CheckBoxOption
                {
                    LabelText = "Disable mouse wheel in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableWheel)
                },
                new CheckBoxOption
                {
                    LabelText = "Disable mouse buttons in play mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.MouseDisableButtons)
                },
                new CheckBoxOption
                {
                    LabelText = "Cursor ripples",
                    Bindable = config.GetBindable<bool>(OsuConfig.CursorRipple)
                },
            };
        }
    }
}