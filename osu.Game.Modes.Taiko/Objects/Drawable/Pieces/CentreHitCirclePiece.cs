using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A circle piece which is used to visualise CentreHit objects.
    /// </summary>
    public class CentreHitCirclePiece : CirclePiece
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.PinkDarker;
        }

        protected override Framework.Graphics.Drawable CreateIcon()
        {
            return new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Size = new Vector2(45f),

                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = 1
                    }
                }
            };
        }
    }
}