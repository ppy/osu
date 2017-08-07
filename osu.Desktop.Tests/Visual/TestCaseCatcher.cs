using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.UI;
using OpenTK;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseCatcher : OsuTestCase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Children = new Drawable[]
            {
                new CatcherArea
                {
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(1, 0.2f),
                }
            };
        }
    }
}
