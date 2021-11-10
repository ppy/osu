// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
    public class UnstableRateCounter : RollingCounter<int>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override double RollingDuration => 750;

        private const float alpha_when_invalid = 0.3f;
        private readonly Bindable<bool> valid = new Bindable<bool>();

        private readonly List<double> hitOffsets = new List<double>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; }

        public UnstableRateCounter()
        {
            Current.Value = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDifficultyCache difficultyCache)
        {
            Colour = colours.BlueLighter;
            valid.BindValueChanged(e =>
                DrawableCount.FadeTo(e.NewValue ? 1 : alpha_when_invalid, 1000, Easing.OutQuint));
        }

        private bool changesUnstableRate(JudgementResult judgement)
            => !(judgement.HitObject.HitWindows is HitWindows.EmptyHitWindows) && judgement.IsHit;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.NewJudgement += onJudgementAdded;
            scoreProcessor.JudgementReverted += onJudgementReverted;
        }

        private void onJudgementAdded(JudgementResult judgement)
        {
            if (!changesUnstableRate(judgement)) return;

            hitOffsets.Add(judgement.TimeOffset);
            updateDisplay();
        }

        private void onJudgementReverted(JudgementResult judgement)
        {
            if (judgement.FailedAtJudgement || !changesUnstableRate(judgement)) return;

            hitOffsets.RemoveAt(hitOffsets.Count - 1);
            updateDisplay();
        }

        private void updateDisplay()
        {
            // At Count = 0, we get NaN, While we are allowing count = 1, it will be 0 since average = offset.
            if (hitOffsets.Count > 0)
            {
                double mean = hitOffsets.Average();
                double squares = hitOffsets.Select(offset => Math.Pow(offset - mean, 2)).Sum();
                Current.Value = (int)(Math.Sqrt(squares / hitOffsets.Count) * 10);
                valid.Value = true;
            }
            else
            {
                Current.Value = 0;
                valid.Value = false;
            }
        }

        protected override IHasText CreateText() => new TextComponent
        {
            Alpha = alpha_when_invalid,
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor == null) return;

            scoreProcessor.NewJudgement -= onJudgementAdded;
            scoreProcessor.JudgementReverted -= onJudgementReverted;
        }

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            private readonly OsuSpriteText text;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 8, fixedWidth: true),
                            Text = "UR",
                            Padding = new MarginPadding { Bottom = 1.5f },
                        }
                    }
                };
            }
        }
    }
}
