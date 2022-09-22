// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class ArgonJudgementPiece : CompositeDrawable, IAnimatableJudgement
    {
        protected readonly HitResult Result;

        protected SpriteText JudgementText { get; private set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public ArgonJudgementPiece(HitResult result)
        {
            Result = result;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                JudgementText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = Result.GetDescription().ToUpperInvariant(),
                    Colour = colours.ForHitResult(Result),
                    Spacing = new Vector2(5, 0),
                    Font = OsuFont.Default.With(size: 20, weight: FontWeight.Bold),
                }
            };
        }

        /// <summary>
        /// Plays the default animation for this judgement piece.
        /// </summary>
        /// <remarks>
        /// The base implementation only handles fade (for all result types) and misses.
        /// Individual rulesets are recommended to implement their appropriate hit animations.
        /// </remarks>
        public virtual void PlayAnimation()
        {
            switch (Result)
            {
                default:
                    JudgementText
                        .ScaleTo(Vector2.One)
                        .ScaleTo(new Vector2(1.2f), 1800, Easing.OutQuint);
                    break;

                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    this.MoveTo(Vector2.Zero);
                    this.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);

                    this.RotateTo(0);
                    this.RotateTo(40, 800, Easing.InQuint);
                    break;
            }

            this.FadeOutFromOne(800);
        }

        public Drawable? GetAboveHitObjectsProxiedContent() => null;
    }
}
