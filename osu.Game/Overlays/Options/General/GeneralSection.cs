using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.General
{
    public class GeneralSection : OptionsSection
    {
        protected override string Header => "General";
        public override FontAwesome Icon => FontAwesome.fa_gear;
    
        public GeneralSection()
        {
            Children = new Drawable[]
            {
                new LoginOptions(),
                new LanguageOptions(),
                new UpdateOptions(),
            };
        }
    }
}

