using osu.Framework;
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
            Children = new Drawable[]
            {
                new SpriteText { Text = "Sensitivity: TODO slider" },
                rawInput = new CheckBoxOption { LabelText = "Raw input" },
                mapRawInput = new CheckBoxOption { LabelText = "Map absolute raw input to the osu! window" },
                new SpriteText { Text = "Confine mouse cursor: TODO dropdown" },
                disableWheel = new CheckBoxOption { LabelText = "Disable mouse wheel in play mode" },
                disableButtons = new CheckBoxOption { LabelText = "Disable mouse buttons in play mode" },
                enableRipples = new CheckBoxOption { LabelText = "Cursor ripples" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                rawInput.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.RawInput);
                mapRawInput.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.AbsoluteToOsuWindow);
                disableWheel.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.MouseDisableWheel);
                disableButtons.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.MouseDisableButtons);
                enableRipples.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.CursorRipple);
            }
        }
    }
}