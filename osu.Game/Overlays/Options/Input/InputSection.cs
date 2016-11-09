using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Input
{
    public class InputSection : OptionsSection
    {
        protected override string Header => "Input";
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

