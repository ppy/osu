using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class InputOptions : OptionsSection
    {
        protected override string Header => "Input";
    
        public InputOptions()
        {
            Children = new Drawable[]
            {
                new MouseOptions(),
                new KeyboardOptions(),
                new OtherInputOptions(),
            };
        }
    }
}

