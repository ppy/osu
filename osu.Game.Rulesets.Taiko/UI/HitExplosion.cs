// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A circle explodes from the hit target to indicate a hitobject has been hit.
    /// </summary>
    internal class HitExplosion : CircularContainer
    {
        /// <summary>
        /// The judgement this hit explosion visualises.
        /// </summary>
        public readonly TaikoJudgement Judgement;

        private readonly Box innerFill;

        public HitExplosion(TaikoJudgement judgement)
        {
            Judgement = judgement;

            Size = new Vector2(TaikoHitObject.DEFAULT_CIRCLE_DIAMETER);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.Both;

            BorderColour = Color4.White;
            BorderThickness = 1;

            Alpha = 0.15f;
            Masking = true;

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
            switch (Judgement.TaikoResult)
            {
                case TaikoHitResult.Good:
                    innerFill.Colour = colours.Green;
                    break;
                case TaikoHitResult.Great:
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

        /// <summary>
        /// Transforms this hit explosion to visualise a secondary hit.
        /// </summary>
        public void VisualiseSecondHit()
        {
            ResizeTo(Size * TaikoHitObject.STRONG_CIRCLE_DIAMETER_SCALE, 50);
        }
    }
}
