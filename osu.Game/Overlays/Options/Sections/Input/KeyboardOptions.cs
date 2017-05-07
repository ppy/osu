// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Input
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
                    Text = "Key Configuration"
                },
            };
        }
    }
}