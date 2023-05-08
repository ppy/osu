// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public partial class ArgonJudgementPiece : JudgementPiece, IAnimatableJudgement
    {
        private RingExplosion? ringExplosion;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public ArgonJudgementPiece(HitResult result)
            : base(result)
        {
            AutoSizeAxes = Axes.Both;

            Origin = Anchor.Centre;
            Y = 160;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (Result.IsHit())
            {
                AddInternal(ringExplosion = new RingExplosion(Result)
                {
                    Colour = colours.ForHitResult(Result),
                });
            }
        }

        protected override SpriteText CreateJudgementText() =>
            new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Blending = BlendingParameters.Additive,
                Spacing = new Vector2(10, 0),
                Font = OsuFont.Default.With(size: 28, weight: FontWeight.Regular),
            };

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
                        .ScaleTo(new Vector2(1.4f), 1800, Easing.OutQuint);
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

            ringExplosion?.PlayAnimation();
        }

        public Drawable? GetAboveHitObjectsProxiedContent() => null;

        private partial class RingExplosion : CompositeDrawable
        {
            private readonly float travel = 52;

            public RingExplosion(HitResult result)
            {
                const float thickness = 4;

                const float small_size = 9;
                const float large_size = 14;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Blending = BlendingParameters.Additive;

                int countSmall = 0;
                int countLarge = 0;

                switch (result)
                {
                    case HitResult.Meh:
                        countSmall = 3;
                        travel *= 0.3f;
                        break;

                    case HitResult.Ok:
                    case HitResult.Good:
                        countSmall = 4;
                        travel *= 0.6f;
                        break;

                    case HitResult.Great:
                    case HitResult.Perfect:
                        countSmall = 4;
                        countLarge = 4;
                        break;
                }

                for (int i = 0; i < countSmall; i++)
                    AddInternal(new RingPiece(thickness) { Size = new Vector2(small_size) });

                for (int i = 0; i < countLarge; i++)
                    AddInternal(new RingPiece(thickness) { Size = new Vector2(large_size) });
            }

            public void PlayAnimation()
            {
                foreach (var c in InternalChildren)
                {
                    const float start_position_ratio = 0.3f;

                    float direction = RNG.NextSingle(0, 360);
                    float distance = RNG.NextSingle(travel / 2, travel);

                    c.MoveTo(new Vector2(
                        MathF.Cos(direction) * distance * start_position_ratio,
                        MathF.Sin(direction) * distance * start_position_ratio
                    ));

                    c.MoveTo(new Vector2(
                        MathF.Cos(direction) * distance,
                        MathF.Sin(direction) * distance
                    ), 600, Easing.OutQuint);
                }

                this.FadeOutFromOne(1000, Easing.OutQuint);
            }

            public partial class RingPiece : CircularContainer
            {
                public RingPiece(float thickness = 9)
                {
                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;

                    Masking = true;
                    BorderThickness = thickness;
                    BorderColour = Color4.White;

                    Child = new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both
                    };
                }
            }
        }
    }
}
