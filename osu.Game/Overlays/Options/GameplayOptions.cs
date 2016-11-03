using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class GameplayOptions : OptionsSection
    {
        public GameplayOptions()
        {
            Header = "Gameplay";
            Children = new Drawable[]
            {
                new GeneralGameplayOptions(),
                new SongSelectGameplayOptions(),
            };
        }
    }
}