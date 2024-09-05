using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarkerMovement : HitMarker
    {
        private Container clickDisplay = null!;
        private Circle mainCircle = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                mainCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.Pink2,
                },
                clickDisplay = new Container
                {
                    Colour = Colours.Yellow,
                    Scale = new Vector2(0.8f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 2,
                            Colour = Color4.White,
                        },
                    },
                },
            };
        }

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            base.OnApply(entry);
            Size = new Vector2(entry.Action != null ? 4 : 2.5f);

            mainCircle.Colour = entry.Action != null ? Colours.Gray4 : Colours.Pink2;

            clickDisplay.Alpha = entry.Action != null ? 1 : 0;
            clickDisplay.Rotation = entry.Action == OsuAction.LeftButton ? 0 : 180;
        }
    }
}
