using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class GameplaySection : OptionsSection
    {
        protected override string Header => "Gameplay";
        public override FontAwesome Icon => FontAwesome.fa_circle_o;
    
        public GameplaySection()
        {
            Children = new Drawable[]
            {
                new GeneralGameplayOptions(),
                new SongSelectGameplayOptions(),
            };
        }
    }
}