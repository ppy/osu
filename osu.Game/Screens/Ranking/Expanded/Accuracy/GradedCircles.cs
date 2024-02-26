// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    public partial class GradedCircles : CompositeDrawable
    {
        private double progress;

        public double Progress
        {
            get => progress;
            set
            {
                progress = value;

                foreach (var circle in circles)
                    circle.RevealProgress = value;
            }
        }

        private readonly Container<GradedCircle> circles;

        public GradedCircles(double accuracyC, double accuracyB, double accuracyA, double accuracyS, double accuracyX)
        {
            InternalChild = circles = new Container<GradedCircle>
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    new GradedCircle(0.0, accuracyC)
                    {
                        Colour = OsuColour.ForRank(ScoreRank.D),
                    },
                    new GradedCircle(accuracyC, accuracyB)
                    {
                        Colour = OsuColour.ForRank(ScoreRank.C),
                    },
                    new GradedCircle(accuracyB, accuracyA)
                    {
                        Colour = OsuColour.ForRank(ScoreRank.B),
                    },
                    new GradedCircle(accuracyA, accuracyS)
                    {
                        Colour = OsuColour.ForRank(ScoreRank.A),
                    },
                    new GradedCircle(accuracyS, accuracyX - AccuracyCircle.VIRTUAL_SS_PERCENTAGE)
                    {
                        Colour = OsuColour.ForRank(ScoreRank.S),
                    },
                    new GradedCircle(accuracyX - AccuracyCircle.VIRTUAL_SS_PERCENTAGE, 1.0)
                    {
                        Colour = OsuColour.ForRank(ScoreRank.X)
                    }
                }
            };
        }

        private partial class GradedCircle : CircularProgress
        {
            public double RevealProgress
            {
                set => Current.Value = Math.Clamp(value, startProgress, endProgress) - startProgress;
            }

            private readonly double startProgress;
            private readonly double endProgress;

            public GradedCircle(double startProgress, double endProgress)
            {
                this.startProgress = startProgress + AccuracyCircle.GRADE_SPACING_PERCENTAGE * 0.5;
                this.endProgress = endProgress - AccuracyCircle.GRADE_SPACING_PERCENTAGE * 0.5;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS;
                Rotation = (float)this.startProgress * 360;
            }
        }
    }
}
