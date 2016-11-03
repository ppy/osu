using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class InputOptions : OptionsSection
    {
        public InputOptions()
        {
            Header = "Input";
            Children = new Drawable[]
            {
                new MouseOptions(),
                new KeyboardOptions(),
                new OtherInputOptions(),
            };
        }
    }
}

