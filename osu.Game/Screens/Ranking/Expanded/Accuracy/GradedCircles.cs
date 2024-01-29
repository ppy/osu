// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    public partial class GradedCircles : BufferedContainer
    {
        private readonly CircularProgress innerMask;

        public GradedCircles(double accuracyC, double accuracyB, double accuracyA, double accuracyS, double accuracyX)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.8f);
            Padding = new MarginPadding(2);
            Children = new Drawable[]
            {
                new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.ForRank(ScoreRank.D),
                    InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS,
                    Current = { Value = accuracyC - AccuracyCircle.NOTCH_WIDTH_PERCENTAGE },
                    Rotation = AccuracyCircle.NOTCH_WIDTH_PERCENTAGE * 0.5f * 360
                },
                new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.ForRank(ScoreRank.C),
                    InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS,
                    Current = { Value = accuracyB - accuracyC - AccuracyCircle.NOTCH_WIDTH_PERCENTAGE },
                    Rotation = (float)accuracyC * 360 + AccuracyCircle.NOTCH_WIDTH_PERCENTAGE * 0.5f * 360
                },
                new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.ForRank(ScoreRank.B),
                    InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS,
                    Current = { Value = accuracyA - accuracyB - AccuracyCircle.NOTCH_WIDTH_PERCENTAGE },
                    Rotation = (float)accuracyB * 360 + AccuracyCircle.NOTCH_WIDTH_PERCENTAGE * 0.5f * 360
                },
                new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.ForRank(ScoreRank.A),
                    InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS,
                    Current = { Value = accuracyS - accuracyA - AccuracyCircle.NOTCH_WIDTH_PERCENTAGE },
                    Rotation = (float)accuracyA * 360 + AccuracyCircle.NOTCH_WIDTH_PERCENTAGE * 0.5f * 360
                },
                new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.ForRank(ScoreRank.S),
                    InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS,
                    Current = { Value = accuracyX - accuracyS - AccuracyCircle.VIRTUAL_SS_PERCENTAGE - AccuracyCircle.NOTCH_WIDTH_PERCENTAGE },
                    Rotation = (float)accuracyS * 360 + AccuracyCircle.NOTCH_WIDTH_PERCENTAGE * 0.5f * 360
                },
                new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.ForRank(ScoreRank.X),
                    InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS,
                    Current = { Value = 1f - (accuracyX - AccuracyCircle.VIRTUAL_SS_PERCENTAGE) - AccuracyCircle.NOTCH_WIDTH_PERCENTAGE },
                    Rotation = (float)(accuracyX - AccuracyCircle.VIRTUAL_SS_PERCENTAGE) * 360 + AccuracyCircle.NOTCH_WIDTH_PERCENTAGE * 0.5f * 360
                },
                new BufferedContainer
                {
                    Name = "Graded circle mask",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(1),
                    Blending = new BlendingParameters
                    {
                        Source = BlendingType.DstColor,
                        Destination = BlendingType.OneMinusSrcColor,
                        SourceAlpha = BlendingType.One,
                        DestinationAlpha = BlendingType.SrcAlpha
                    },
                    Child = innerMask = new CircularProgress
                    {
                        RelativeSizeAxes = Axes.Both,
                        InnerRadius = AccuracyCircle.RANK_CIRCLE_RADIUS - 0.02f,
                    }
                }
            };
        }

        public void Transform()
        {
            using (BeginDelayedSequence(AccuracyCircle.RANK_CIRCLE_TRANSFORM_DELAY))
                innerMask.FillTo(1f, AccuracyCircle.RANK_CIRCLE_TRANSFORM_DURATION, AccuracyCircle.ACCURACY_TRANSFORM_EASING);
        }
    }
}
