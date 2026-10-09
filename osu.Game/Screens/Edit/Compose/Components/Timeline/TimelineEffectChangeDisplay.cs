// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    /// <summary>
    /// The part of the timeline that displays effect points.
    /// </summary>
    public partial class TimelineEffectChangeDisplay : TimelinePart<TimelineEffectChangeDisplay.EffectPointPiece>
    {
        [Resolved]
        private Timeline timeline { get; set; } = null!;

        /// <summary>
        /// The visible time/position range of the timeline.
        /// </summary>
        private (float min, float max) visibleRange = (float.MinValue, float.MaxValue);

        private readonly Cached groupCache = new Cached();

        private ControlPointInfo controlPointInfo = null!;

        private bool showScrollSpeed;

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            showScrollSpeed = beatmap.BeatmapInfo.Ruleset.CreateInstance().EditorShowScrollSpeed;

            beatmap.ControlPointInfo.ControlPointsChanged += () => groupCache.Invalidate();
            controlPointInfo = beatmap.ControlPointInfo;
        }

        protected override void Update()
        {
            base.Update();

            if (DrawWidth <= 0) return;

            (float, float) newRange = (
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopLeft).X - EffectPointPiece.WIDTH) / DrawWidth * Content.RelativeChildSize.X,
                (ToLocalSpace(timeline.ScreenSpaceDrawQuad.TopRight).X + EffectPointPiece.WIDTH) / DrawWidth * Content.RelativeChildSize.X);

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
            foreach (EffectPointPiece drawableGroup in this)
            {
                if (!controlPointInfo.EffectPoints.Contains(drawableGroup.Point) || !shouldBeVisible(drawableGroup.Point))
                    drawableGroup.Expire();
            }

            foreach (EffectControlPoint point in controlPointInfo.EffectPoints)
                attemptAddEffectPoint(point);
        }

        private void attemptAddEffectPoint(EffectControlPoint point)
        {
            if (!shouldBeVisible(point))
                return;

            foreach (var child in this)
            {
                if (ReferenceEquals(child.Point, point))
                    return;
            }

            Add(new EffectPointPiece(point, showScrollSpeed));
        }

        private bool shouldBeVisible(EffectControlPoint point) => point.Time >= visibleRange.min && point.Time <= visibleRange.max;

        public partial class EffectPointPiece : CompositeDrawable, IHasTooltip
        {
            public const float WIDTH = 16;

            public LocalisableString TooltipText
            {
                get
                {
                    string kiai = Point.KiaiMode ? @"kiai on" : @"kiai off";

                    if (!showScrollSpeed)
                        return kiai;

                    return $"{Point.ScrollSpeed:n2}x scroll speed, {kiai}";
                }
            }

            public readonly EffectControlPoint Point;

            private readonly BindableNumber<double> scrollSpeed;
            private readonly Bindable<bool> kiaiMode;
            private readonly bool showScrollSpeed;

            private SpriteIcon star = null!;
            private OsuSpriteText label = null!;

            public EffectPointPiece(EffectControlPoint effectPoint, bool showScrollSpeed)
            {
                RelativePositionAxes = Axes.X;

                RelativeSizeAxes = Axes.Y;
                Width = WIDTH;

                Origin = Anchor.TopRight;

                Point = effectPoint;
                this.showScrollSpeed = showScrollSpeed;

                scrollSpeed = effectPoint.ScrollSpeedBindable.GetBoundCopy();
                kiaiMode = effectPoint.KiaiModeBindable.GetBoundCopy();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Yellow,
                        Masking = true,
                        CornerRadius = TimelineTickDisplay.TICK_WIDTH / 2,
                        Child = new Box
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                    label = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Rotation = 90,
                        Padding = new MarginPadding { Horizontal = 2 },
                        Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold),
                        Colour = colours.B5,
                    },
                    star = new SpriteIcon
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = 3 },
                        Size = new Vector2(12),
                        Icon = FontAwesome.Regular.Star,
                        Colour = colours.B5,
                    },
                };

                if (!showScrollSpeed)
                    label.Hide();

                kiaiMode.BindValueChanged(kiai => star.Alpha = kiai.NewValue ? 1 : 0, true);

                scrollSpeed.BindValueChanged(speed => label.Text = $"{speed.NewValue:n2}x", true);
            }

            protected override void Update()
            {
                base.Update();
                X = (float)Point.Time;
            }
        }
    }
}
