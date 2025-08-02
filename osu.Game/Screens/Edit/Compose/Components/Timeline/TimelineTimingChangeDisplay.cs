// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    /// <summary>
    /// The part of the timeline that displays the control points.
    /// </summary>
    public partial class TimelineTimingChangeDisplay : TimelinePart<TimelineTimingChangeDisplay.TimingPointPiece>
    {
        [Resolved]
        private Timeline timeline { get; set; } = null!;

        /// <summary>
        /// The visible time/position range of the timeline.
        /// </summary>
        private (float min, float max) visibleRange = (float.MinValue, float.MaxValue);

        private readonly Cached groupCache = new Cached();

        private ControlPointInfo controlPointInfo = null!;

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            beatmap.ControlPointInfo.ControlPointsChanged += () => groupCache.Invalidate();
            controlPointInfo = beatmap.ControlPointInfo;
        }

        protected override void Update()
        {
            base.Update();

            if (DrawWidth <= 0) return;

            (float, float) newRange = (
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopLeft).X - TimingPointPiece.WIDTH) / DrawWidth * Content.RelativeChildSize.X,
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopRight).X + TimingPointPiece.WIDTH) / DrawWidth * Content.RelativeChildSize.X);

            if (visibleRange != newRange)
            {
                visibleRange = newRange;
                groupCache.Invalidate();
            }

            if (!groupCache.IsValid)
            {
                recreateDrawableGroups();
                groupCache.Validate();
            }
        }

        private void recreateDrawableGroups()
        {
            // Remove groups outside the visible range (or timing points which have since been removed from the beatmap).
            foreach (TimingPointPiece drawableGroup in this)
            {
                if (!controlPointInfo.TimingPoints.Contains(drawableGroup.Point) || !shouldBeVisible(drawableGroup.Point))
                    drawableGroup.Expire();
            }

            // Add remaining / new ones.
            foreach (TimingControlPoint t in controlPointInfo.TimingPoints)
                attemptAddTimingPoint(t);
        }

        private void attemptAddTimingPoint(TimingControlPoint point)
        {
            if (!shouldBeVisible(point))
                return;

            foreach (var child in this)
            {
                if (ReferenceEquals(child.Point, point))
                    return;
            }

            Add(new TimingPointPiece(point));
        }

        private bool shouldBeVisible(TimingControlPoint point) => point.Time >= visibleRange.min && point.Time <= visibleRange.max;

        public partial class TimingPointPiece : CompositeDrawable
        {
            public const float WIDTH = 16;

            public readonly TimingControlPoint Point;

            private readonly BindableNumber<double> beatLength;

            protected OsuSpriteText Label { get; private set; } = null!;

            public TimingPointPiece(TimingControlPoint timingPoint)
            {
                RelativePositionAxes = Axes.X;

                RelativeSizeAxes = Axes.Y;
                Width = WIDTH;

                Origin = Anchor.TopRight;

                Point = timingPoint;

                beatLength = timingPoint.BeatLengthBindable.GetBoundCopy();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Point.GetRepresentingColour(colours),
                        Masking = true,
                        CornerRadius = TimelineTickDisplay.TICK_WIDTH / 2,
                        Child = new Box
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                    Label = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Rotation = 90,
                        Padding = new MarginPadding { Horizontal = 2 },
                        Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold),
                    }
                };

                beatLength.BindValueChanged(beatLength =>
                {
                    Label.Text = $"{60000 / beatLength.NewValue:n1} BPM";
                }, true);
            }

            protected override void Update()
            {
                base.Update();
                X = (float)Point.Time;
            }
        }
    }
}
