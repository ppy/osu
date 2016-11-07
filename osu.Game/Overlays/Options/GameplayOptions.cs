using System;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class GameplayOptions : OptionsSection
    {
        protected override string Header => "Gameplay";
        public override FontAwesome Icon => FontAwesome.fa_circle_o;
    
        public GameplayOptions()
        {
            Children = new Drawable[]
            {
                new GeneralGameplayOptions(),
                new SongSelectGameplayOptions(),
            };
        }
    }
}