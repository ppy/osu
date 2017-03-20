using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A circle piece which is used to visualise DrumRoll objects.
    /// </summary>
    public class DrumRollCirclePiece : CirclePiece
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.YellowDark;
        }
    }
}