using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarkerMovement : HitMarker
    {
        public HitMarkerMovement()
        {
            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = OsuColour.Gray(0.2f),
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1.2f)
                },
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            base.OnApply(entry);

            Size = new Vector2(entry.Action != null ? 4 : 3);
        }
    }
}
