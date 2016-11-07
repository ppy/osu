using System;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class InputOptions : OptionsSection
    {
        protected override string Header => "Input";
        public override FontAwesome Icon => FontAwesome.fa_keyboard_o;
    
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

