// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
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
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class UnstableRateCounter : RollingCounter<double>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override bool IsRollingProportional => true;

        protected override double RollingDuration => 750;

        private const float alpha_when_invalid = 0.3f;

        private List<double> hitList = new List<double>();

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private ScoreProcessor scoreProcessor { get; set; }

        private readonly CancellationTokenSource loadCancellationSource = new CancellationTokenSource();
        public UnstableRateCounter()
        {
            Current.Value = 0.0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDifficultyCache difficultyCache)
        {
            Colour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (scoreProcessor != null)
            {
                scoreProcessor.NewJudgement += onJudgementAdded;
                scoreProcessor.JudgementReverted += onJudgementChanged;
            }
        }

        private bool isValid;
        private void setValid(bool valid)
        {
            if (isValid == valid) return;
            DrawableCount.FadeTo(isValid ? 1 : alpha_when_invalid, 1000, Easing.OutQuint);
            isValid = valid;
        }

        private void onJudgementAdded(JudgementResult judgement)
        {
            if (!(judgement.HitObject.HitWindows is HitWindows.EmptyHitWindows) && judgement.IsHit)
            {
                hitList.Add(judgement.TimeOffset);
            }
            updateUR();
        }

        // Only populate via the score if the user has moved the current location. 
        private void onJudgementChanged(JudgementResult judgement)
        {
            ScoreInfo currentScore = new ScoreInfo();
            scoreProcessor.PopulateScore(currentScore);
            hitList = currentScore.HitEvents.Where(e => !(e.HitObject.HitWindows is HitWindows.EmptyHitWindows) && e.Result.IsHit())
                                            .Select(ev => ev.TimeOffset).ToList<double>();
            updateUR();
        }

        private void updateUR()
        {
            if (hitList.Count > 0)
            {
                double mean = hitList.Average();
                double squares = hitList.Select(offset => Math.Pow(offset - mean, 2)).Sum();
                Current.Value = Math.Sqrt(squares / hitList.Count) * 10;
                setValid(true);
            }
            else
            {
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

            if (scoreProcessor != null)
            {
                scoreProcessor.NewJudgement -= onJudgementAdded;
                scoreProcessor.JudgementReverted -= onJudgementChanged;
            }
            loadCancellationSource?.Cancel();
        }

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => intPart.Text;
                set {
                    //Not too sure about this, is there a better way to go about doing this?
                    splitValue = value.ToString().Split('.');
                    intPart.Text = splitValue[0];
                    decimalPart.Text = $".{splitValue[1]} UR";
                }
            }

            private string[] splitValue;
            private readonly OsuSpriteText intPart;
            private readonly OsuSpriteText decimalPart;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    //Spacing = new Vector2(2),
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
                            Text = @" UR",
                            Font = OsuFont.Numeric.With(size: 8, fixedWidth: true),
                            Padding = new MarginPadding { Bottom = 1.5f },
                        }
                    }
                };
            }
        }
    }
}
