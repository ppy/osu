//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Input
{
    public class InputSection : OptionsSection
    {
        public override string Header => "Input";
        public override FontAwesome Icon => FontAwesome.fa_keyboard_o;

        public InputSection()
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

