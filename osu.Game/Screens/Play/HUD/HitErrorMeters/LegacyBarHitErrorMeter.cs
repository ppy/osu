// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/baa8705f782c0de2b10a7387d78014c61c8b17fb/osu!/GameModes/Play/Components/ScoreMeterError.cs"/>
    public partial class LegacyBarHitErrorMeter : HitErrorMeter
    {
        private const float bar_height = 3;

        private (HitResult result, double length)[] hitWindows = null!;
        private DrawablePool<JudgementLine> judgementLinePool = null!;
        private Container judgementContainer = null!;
        private Triangle arrow = null!;

        private double maxHitWindow = 1;
        private float floatingError;

        [BackgroundDependencyLoader]
        private void load()
        {
            Container colourBarContainer;

            hitWindows = HitWindows.GetAllAvailableWindows().Where(w => w.result.IsHit()).ToArray();

            AutoSizeAxes = Axes.X;
            Height = (bar_height * 4) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.6f,
                },
                colourBarContainer = new Container
                {
                    AutoSizeAxes = Axes.X,
                    Height = bar_height * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1.5f, bar_height * 4) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                },
                judgementLinePool = new DrawablePool<JudgementLine>(50),
                judgementContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                arrow = new Triangle
                {
                    Size = new Vector2(17, 8) * 0.6f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    RelativePositionAxes = Axes.X,
                    Scale = new Vector2(1, -1),
                }
            };

            foreach (var hitWindow in hitWindows)
            {
                maxHitWindow = Math.Max(maxHitWindow, hitWindow.length);

                colourBarContainer.Add(new Box
                {
                    Size = new Vector2((float)hitWindow.length, bar_height) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = GetColourForHitResult(hitWindow.result),
                });
            }
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit || judgement.HitObject.HitWindows?.WindowFor(HitResult.Miss) == 0)
                return;

            if (!judgement.Type.IsScorable() || judgement.Type.IsBonus())
                return;

            float relativePosition = getRelativeJudgementPosition(judgement.TimeOffset);

            judgementLinePool.Get(drawableJudgement =>
            {
                drawableJudgement.X = relativePosition;
                drawableJudgement.Colour = GetColourForHitResult(judgement.Type);

                judgementContainer.Add(drawableJudgement);
            });

            floatingError = floatingError * 0.8f + relativePosition * 0.2f;
            arrow.MoveToX(floatingError, 800, Easing.Out);
        }

        private float getRelativeJudgementPosition(double value) => Math.Clamp((float)(value / maxHitWindow) / 2, -0.5f, 0.5f);

        public override void Clear()
        {
            foreach (var j in judgementContainer)
            {
                j.ClearTransforms();
                j.Expire();
            }
        }

        internal partial class JudgementLine : PoolableDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Y;
                Width = 3;
                RelativePositionAxes = Axes.X;
                Blending = BlendingParameters.Additive;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChild = new Box { RelativeSizeAxes = Axes.Both };
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                this.FadeTo(0.4f)
                    .Then()
                    .FadeOut(10000)
                    .Expire();
            }
        }
    }
}
