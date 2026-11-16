// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded.Accuracy;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class GradedCirclesV2 : CircularContainer
    {
        private double progress;

        public double Progress
        {
            get => progress;
            set
            {
                progress = value;

                foreach (var circle in Children.OfType<GradedCircle>())
                    circle.RevealProgress = value;
            }
        }

        public GradedCirclesV2(ScoreProcessor scoreProcessor)
        {
            double accuracyC = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.C);
            double accuracyB = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.B);
            double accuracyA = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.A);
            double accuracyS = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.S);
            double accuracyX = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.X);

            Children = new[]
            {
                // sort of a hack.
                // the goal is to have the rings Z-order-sorted like so:
                // D < SS < S < A < B < C < D
                // this is circular, so the D ring is split in two halves;
                // the part of D touching SS is *under* SS,
                // and the part of D touching C is *above* SS
                new GradedCircle(ScoreRank.D, 0.0, accuracyC / 2),
                new GradedCircle(ScoreRank.X, accuracyX - AccuracyCircle.VIRTUAL_SS_PERCENTAGE, 1.0),
                new GradedCircle(ScoreRank.S, accuracyS, accuracyX - AccuracyCircle.VIRTUAL_SS_PERCENTAGE),
                new GradedCircle(ScoreRank.A, accuracyA, accuracyS),
                new GradedCircle(ScoreRank.B, accuracyB, accuracyA),
                new GradedCircle(ScoreRank.C, accuracyC, accuracyB),
                new GradedCircle(ScoreRank.D, accuracyC / 2, accuracyC),
            };
        }

        private partial class GradedCircle : CircularProgress
        {
            public double RevealProgress
            {
                set
                {
                    Progress = Math.Clamp(value, startProgress, endProgress) - startProgress;
                    Alpha = Progress > 0 ? 1 : 0;
                }
            }

            private readonly double startProgress;
            private readonly double endProgress;

            public GradedCircle(ScoreRank scoreRank, double startProgress, double endProgress)
            {
                this.startProgress = startProgress;
                this.endProgress = endProgress;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                InnerRadius = 0.08f;
                Rotation = (float)this.startProgress * 360;
                RoundedCaps = true;
                Colour = OsuColour.ForRank(scoreRank);
            }
        }
    }
}
