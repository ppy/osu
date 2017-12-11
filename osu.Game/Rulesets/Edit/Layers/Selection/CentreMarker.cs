using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// Represents the centre of a <see cref="MarkerContainer"/>.
    /// </summary>
    public class CentreMarker : CompositeDrawable
    {
        private const float marker_size = 10;
        private const float line_width = 2;

        public CentreMarker()
        {
            Size = new Vector2(marker_size);

            InternalChildren = new[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = line_width
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Width = line_width
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }
    }
}
