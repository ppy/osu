using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarkerClick : HitMarker
    {
        private Container clickDisplay = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.125f),
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Colour = Colours.Gray5,
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.Gray5,
                    Masking = true,
                    BorderThickness = 2.2f,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0,
                    },
                },
                clickDisplay = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.Yellow,
                    Scale = new Vector2(0.95f),
                    Width = 0.5f,
                    Masking = true,
                    BorderThickness = 2,
                    BorderColour = Color4.White,
                    Child = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 2,
                        Masking = true,
                        BorderThickness = 2,
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                            AlwaysPresent = true,
                            Alpha = 0,
                        },
                    }
                }
            };

            Size = new Vector2(16);
        }

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            base.OnApply(entry);

            clickDisplay.Alpha = entry.Action != null ? 1 : 0;
            clickDisplay.Rotation = entry.Action == OsuAction.LeftButton ? 0 : 180;
        }
    }
}
