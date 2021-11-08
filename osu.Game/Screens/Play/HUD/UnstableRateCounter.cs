// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class UnstableRateCounter : RollingCounter<double>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override double RollingDuration => 750;

        private const float alpha_when_invalid = 0.3f;

        private readonly List<double> hitOffsets = new List<double>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; }

        public UnstableRateCounter()
        {
            Current.Value = 0.0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDifficultyCache difficultyCache)
        {
            Colour = colours.BlueLighter;
        }

        private bool isUrInvalid(JudgementResult judgement)
        {
            return judgement.HitObject.HitWindows is HitWindows.EmptyHitWindows || !judgement.IsHit;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.NewJudgement += onJudgementAdded;
            scoreProcessor.JudgementReverted += onJudgementReverted;
        }

        private bool isValid;

        private void setValid(bool valid)
        {
            if (isValid == valid) return;

            DrawableCount.FadeTo(valid ? 1 : alpha_when_invalid, 1000, Easing.OutQuint);
            isValid = valid;
        }

        private void onJudgementAdded(JudgementResult judgement)
        {
            if (isUrInvalid(judgement)) return;

            hitOffsets.Add(judgement.TimeOffset);
            updateUr();
        }

        private void onJudgementReverted(JudgementResult judgement)
        {
            if (judgement.FailedAtJudgement || isUrInvalid(judgement)) return;

            hitOffsets.RemoveAt(hitOffsets.Count - 1);
            updateUr();
        }

        private void updateUr()
        {
            // At Count = 0, we get NaN, While we are allowing count = 1, it will be 0 since average = offset.
            if (hitOffsets.Count > 0)
            {
                double mean = hitOffsets.Average();
                double squares = hitOffsets.Select(offset => Math.Pow(offset - mean, 2)).Sum();
                Current.Value = Math.Sqrt(squares / hitOffsets.Count) * 10;
                setValid(true);
            }
            else
            {
                Current.Value = 0;
                setValid(false);
            }
        }

        protected override LocalisableString FormatCount(double count)
        {
            return count.ToString("0.00");
        }

        protected override IHasText CreateText() => new TextComponent
        {
            Alpha = alpha_when_invalid,
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            scoreProcessor.NewJudgement -= onJudgementAdded;
            scoreProcessor.JudgementReverted -= onJudgementReverted;
        }

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => fullValue.ToLocalisableString("0.00 UR");
                set
                {
                    fullValue = Convert.ToDouble(value.ToString());
                    intPart.Text = fullValue.ToLocalisableString("0");
                    decimalPart.Text = (fullValue - Math.Truncate(fullValue))
                        .ToLocalisableString(".00 UR");
                }
            }

            private double fullValue;

            private readonly OsuSpriteText intPart;
            private readonly OsuSpriteText decimalPart;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        intPart = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        decimalPart = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 8, fixedWidth: true),
                            Padding = new MarginPadding { Bottom = 1.5f },
                        }
                    }
                };
            }
        }
    }
}
