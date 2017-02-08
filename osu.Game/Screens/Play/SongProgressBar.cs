using System;
using osu.Game.Overlays;
namespace osu.Game.Screens.Play
{
    public class SongProgressBar : DragBar
    {
        public static readonly int BAR_HEIGHT = 5;

        public SongProgressBar()
        {
            Colour = SongProgress.FILL_COLOUR;
            Height = BAR_HEIGHT;
        }
    }
}
