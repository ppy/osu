// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Judgements
{
    public class DefaultJudgementPiece : CompositeDrawable, IAnimatableJudgement
    {
        protected SpriteText JudgementText { get; }

        [Resolved]
        private OsuColour colours { get; set; }

        public DefaultJudgementPiece()
        {
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                JudgementText = new OsuSpriteText
                {
                    Font = OsuFont.Numeric.With(size: 20),
                    Scale = new Vector2(0.85f, 1),
                }
            };
        }

        public virtual void PlayAnimation(HitResult result)
        {
            JudgementText.Text = result.GetDescription().ToUpperInvariant();
            JudgementText.Colour = colours.ForHitResult(result);

            this.RotateTo(0);
            this.MoveTo(Vector2.Zero);

            switch (result)
            {
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    this.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);

                    this.RotateTo(40, 800, Easing.InQuint);
                    break;

                default:
                    this.ScaleTo(0.9f);
                    this.ScaleTo(1, 500, Easing.OutElastic);
                    break;
            }
        }
    }
}
