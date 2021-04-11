using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers.Bars
{
    public class FallBar : BasicBar
    {
        protected override IEnumerable<Drawable> ColourReceptors => new[] { main, piece };

        private Container mainBar;
        private Container fallingPiece;
        private Box main;
        private Box piece;

        protected override Drawable CreateContent() => new Container
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                mainBar = new Container
                {
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Child = main = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        EdgeSmoothness = Vector2.One,
                    }
                },
                fallingPiece = new Container
                {
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 2,
                    Child = piece = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        EdgeSmoothness = Vector2.One,
                    }
                }
            },
        };

        public override void SetValue(float amplitudeValue, float valueMultiplier, int smoothness)
        {
            var newValue = ValueFormula(amplitudeValue, valueMultiplier);

            if (newValue > mainBar.Height)
            {
                mainBar.ResizeHeightTo(newValue)
                    .Then()
                    .ResizeHeightTo(0, smoothness);
            }

            if (mainBar.Height > -fallingPiece.Y)
            {
                fallingPiece.MoveToY(-newValue)
                    .Then()
                    .MoveToY(0, smoothness * 6);
            }
        }
    }
}
