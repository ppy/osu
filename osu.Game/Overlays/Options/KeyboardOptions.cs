using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class KeyboardOptions : OptionsSubsection
    {
        public KeyboardOptions()
        {
            Header = "Keyboard";
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