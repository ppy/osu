//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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