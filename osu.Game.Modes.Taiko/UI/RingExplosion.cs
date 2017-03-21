using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;

namespace osu.Game.Modes.Taiko.UI
{
    internal class RingExplosion : CircularContainer
    {
        public TaikoScoreResult ScoreResult;

        private Box innerFill;

        public RingExplosion()
        {
            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.Both;

            BorderColour = Color4.White;
            BorderThickness = 1;

            Alpha = 0.15f;

            Children = new[]
            {
                innerFill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            switch (ScoreResult)
            {
                default:
                    break;
                case TaikoScoreResult.Good:
                    innerFill.Colour = colours.Green;
                    break;
                case TaikoScoreResult.Great:
                    innerFill.Colour = colours.Blue;
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScaleTo(5f, 1000, EasingTypes.OutQuint);
            FadeOut(500);

            Expire();
        }
    }
}
