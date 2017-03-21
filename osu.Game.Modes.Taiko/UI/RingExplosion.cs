// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        public TaikoJudgementInfo Judgement;

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
            if (Judgement.SecondHit)
                Size *= 1.5f;

            switch (Judgement.Score)
            {
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
