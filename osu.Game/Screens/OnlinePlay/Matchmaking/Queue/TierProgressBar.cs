// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Layout;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class TierProgressBar : Container
    {
        private static readonly IEasingFunction rating_change = new CubicBezierEasingFunction(0, 0, 0, 1);

        private static readonly IEasingFunction flow_move = new CubicBezierEasingFunction(0.15, 0.6, 0, 1);
        private static readonly IEasingFunction flow_move_entering = new CubicBezierEasingFunction(0, 0,0, 1);

        private static readonly IEasingFunction next_div_display_fade = new CubicBezierEasingFunction(0.5, 0, 0.5, 1);

        private readonly BindableInt currentRating = new BindableInt();
        private readonly BindableFloat currentRatingInstantaneous = new BindableFloat();

        private readonly Bindable<APIRankedPlayDivision> currentDivision = new Bindable<APIRankedPlayDivision>();

        public required IEnumerable<APIRankedPlayDivision> Divisions
        {
            private get;
            set
            {
                if (Transforms.Any() || segmentFlow.Transforms.Any())
                    throw new InvalidOperationException("Cannot set divisions while a transform is in progress.");

                field = value;

                lowerBound = field.Min(d => d.StartRating);
                upperBound = field.Max(d => d.EndRating);

                currentRatingInstantaneous.Value = currentRating.Value = Math.Clamp(currentRating.Value, lowerBound, upperBound);

                segmentFlow.Clear();
                segmentFlow.AddRange(field.Select(d => new DivisionSegment(d)
                {
                    CurrentRating = { BindTarget = currentRatingInstantaneous },
                }));

                updateSegmentSizes();
            }
        }

        private const int displayed_rating_range = 800;

        private int lowerBound;
        private int upperBound;

        /// <summary>
        /// The width currently being taken up by a single rating point.
        /// </summary>
        private float widthPerRating;

        private readonly Box background;
        private readonly Container segmentWindow;
        private readonly FillFlowContainer<DivisionSegment> segmentFlow;
        private readonly RatingToNextDivisionDisplay nextDivDisplay;

        private readonly LayoutValue layout = new LayoutValue(Invalidation.DrawSize);

        public TierProgressBar()
        {
            // Set fixed height so that we don't get any extraneous layout invalidations
            // due to the `nextDivDisplay` fade in/out triggering autosizing;
            Height = 56;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                    Masking = true,
                    CornerRadius = 15,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(0, 3),
                        Radius = 3,
                        Colour = Colour4.Black.Opacity(0.25f),
                    },
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                segmentWindow = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        segmentFlow = new FillFlowContainer<DivisionSegment>
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 10, Horizontal = 2 },
                            Direction = FillDirection.Horizontal,
                        },
                    },
                },
                nextDivDisplay = new RatingToNextDivisionDisplay
                {
                    CurrentRating = { BindTarget = currentRating },
                    CurrentDivision = { BindTarget = currentDivision },
                    Alpha = 0,
                    Flipped = true,
                },
            };

            AddLayout(layout);
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background5;
        }

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
            {
                // Cancel animations if screen has been resized.
                // Positioning of `segmentFlow` inside `segmentWindow` is dependent on
                // the `DrawWidth` of the latter at the moment the transform begins,
                // so we can no longer rely on it after a resize.
                FinishTransforms(true);

                updateSegmentSizes();
                layout.Validate();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateSegmentSizes();
        }

        public void SetRating(int? value = null, bool entering = false)
        {
            int rating = value ?? lowerBound;

            if (currentRating.Value == rating)
                return;

            if (rating < lowerBound || rating > upperBound)
                throw new ArgumentException("Rating is outside the division bounds.");

            if (entering)
            {
                int initialRating = Math.Max(rating - 300, lowerBound);
                currentRatingInstantaneous.Value = initialRating;
                segmentFlow.MoveToX(calculateContainerOffset(initialRating));
            }

            currentRating.Value = rating;

            // TODO: This is currently a bindable only so that `RatingToNextDivisionDisplay` has
            //       an up to date source of the division for the current rating. It should be
            //       refactored to fetch that data from some centralized source of division data,
            //       once it exists.
            currentDivision.Value = Divisions.First(d => d.StartRating <= rating && d.EndRating >= rating);

            int duration = entering ? 1500 : 1600;
            IEasingFunction flowEasing = entering
                ? flow_move_entering
                : flow_move;

            this.TransformBindableTo(currentRatingInstantaneous, rating, duration, rating_change);
            segmentFlow.MoveToX(calculateContainerOffset(rating), duration, flowEasing);

            var nextDivDisplayTransform = nextDivDisplay.FadeOut(entering ? 0 : 100, next_div_display_fade);

            if (currentDivision.Value?.Tier != RankingTier.Lustrous)
            {
                nextDivDisplayTransform
                    .Then()
                    .Schedule(() =>
                    {
                        updateToNextDivPosition();
                        nextDivDisplay.UpdateLabel();
                    })
                    .Delay(duration - 600)
                    .FadeIn(200, next_div_display_fade);
            }
        }

        private void updateSegmentSizes()
        {
            // We want the amount of displayed divisions/segments to remain consistent
            // regardless of screen dimensions.
            widthPerRating = segmentWindow.DrawWidth / displayed_rating_range;
            segmentFlow.X = calculateContainerOffset(currentRating.Value);

            foreach (var segment in segmentFlow)
                segment.Width = segment.Division.Width * widthPerRating;
        }

        private void updateToNextDivPosition()
        {
            if (currentRating.Value - lowerBound > displayed_rating_range / 2)
            {
                nextDivDisplay.Flipped = false;
                nextDivDisplay.Anchor = Anchor.TopCentre;
                nextDivDisplay.X = 0;
                return;
            }

            nextDivDisplay.Flipped = true;
            nextDivDisplay.Anchor = Anchor.TopLeft;
            nextDivDisplay.X = (currentRating.Value - lowerBound) * widthPerRating + segmentFlow.Padding.Left;
        }

        private float calculateContainerOffset(float rating)
        {
            float filledBarLength = (rating - lowerBound) * widthPerRating + segmentFlow.Padding.Left;
            float totalBarLength = (upperBound - lowerBound) * widthPerRating + segmentFlow.Padding.TotalHorizontal;

            return Math.Clamp(-filledBarLength + segmentWindow.DrawWidth / 2, -totalBarLength + segmentWindow.DrawWidth, 0);
        }
    }
}
