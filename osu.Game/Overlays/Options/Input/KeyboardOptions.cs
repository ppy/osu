using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Input
{
    public class KeyboardOptions : OptionsSubsection
    {
        protected override string Header => "Keyboard";

        public KeyboardOptions()
        {
            Children = new Drawable[]
            {
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Change keyboard bindings"
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "osu!mania layout"
                }
            };
        }
    }
}