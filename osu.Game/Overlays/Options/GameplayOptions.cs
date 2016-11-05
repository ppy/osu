using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class GameplayOptions : OptionsSection
    {
        protected override string Header => "Gameplay";
    
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