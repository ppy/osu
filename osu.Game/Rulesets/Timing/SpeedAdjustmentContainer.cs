// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A container that provides the speed adjustments defined by <see cref="MultiplierControlPoint"/>s to affect the scroll speed
    /// of container <see cref="DrawableHitObject"/>s.
    /// </summary>
    public class SpeedAdjustmentContainer : Container<DrawableHitObject>
    {
        /// <summary>
        /// Gets or sets the range of time that is visible by the length of the scrolling axes.
        /// </summary>
        public readonly Bindable<double> VisibleTimeRange = new Bindable<double> { Default = 1000 };

        /// <summary>
        /// Whether to reverse the scrolling direction is reversed.
        /// </summary>
        public readonly BindableBool Reversed = new BindableBool();

        protected override Container<DrawableHitObject> Content => content;
        private readonly Container<DrawableHitObject> content;

        /// <summary>
        /// The axes which the content of this container will scroll through.
        /// </summary>
        public Axes ScrollingAxes
        {
            get { return scrollingContainer.ScrollingAxes; }
            set { scrollingContainer.ScrollingAxes = value; }
        }

        public override bool RemoveWhenNotAlive => false;
        protected override bool RequiresChildrenUpdate => true;

        /// <summary>
        /// The <see cref="MultiplierControlPoint"/> that defines the speed adjustments.
        /// </summary>
        public readonly MultiplierControlPoint ControlPoint;

        private readonly ScrollingContainer scrollingContainer;

        /// <summary>
        /// Creates a new <see cref="SpeedAdjustmentContainer"/>.
        /// </summary>
        /// <param name="controlPoint">The <see cref="MultiplierControlPoint"/> that defines the speed adjustments.</param>
        public SpeedAdjustmentContainer(MultiplierControlPoint controlPoint)
        {
            ControlPoint = controlPoint;
            RelativeSizeAxes = Axes.Both;

            scrollingContainer = CreateScrollingContainer();
            scrollingContainer.ControlPoint = ControlPoint;
            scrollingContainer.VisibleTimeRange.BindTo(VisibleTimeRange);

            AddInternal(content = scrollingContainer);
        }

        protected override void Update()
        {
            float multiplier = (float)ControlPoint.Multiplier;

            // The speed adjustment happens by modifying our size by the multiplier while maintaining the visible time range as the relatve size for our children
            Size = new Vector2((ScrollingAxes & Axes.X) > 0 ? multiplier : 1, (ScrollingAxes & Axes.Y) > 0 ? multiplier : 1);

            if (Reversed)
            {
                RelativeChildSize = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)-VisibleTimeRange : 1, (ScrollingAxes & Axes.Y) > 0 ? (float)-VisibleTimeRange : 1);
                RelativeChildOffset = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)VisibleTimeRange : 0, (ScrollingAxes & Axes.Y) > 0 ? (float)VisibleTimeRange : 0);
                Origin = Anchor = Anchor.BottomRight;
            }
            else
            {
                RelativeChildSize = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)VisibleTimeRange : 1, (ScrollingAxes & Axes.Y) > 0 ? (float)VisibleTimeRange : 1);
                RelativeChildOffset = Vector2.Zero;
                Origin = Anchor = Anchor.TopLeft;
            }
        }

        public override double LifetimeStart => ControlPoint.StartTime - VisibleTimeRange;
        public override double LifetimeEnd => ControlPoint.StartTime + scrollingContainer.Duration + VisibleTimeRange;

        public override void Add(DrawableHitObject drawable)
        {
            var scrollingHitObject = drawable as IScrollingHitObject;

            if (scrollingHitObject != null)
            {
                scrollingHitObject.LifetimeOffset.BindTo(VisibleTimeRange);
                scrollingHitObject.ScrollingAxes = ScrollingAxes;
            }

            base.Add(drawable);
        }

        /// <summary>
        /// Whether a point in time falls within this <see cref="SpeedAdjustmentContainer"/>s affecting timespan.
        /// </summary>
        public bool CanContain(double startTime) => ControlPoint.StartTime <= startTime;

        /// <summary>
        /// Creates the <see cref="ScrollingContainer"/> which contains the scrolling <see cref="DrawableHitObject"/>s of this container.
        /// </summary>
        /// <returns>The <see cref="ScrollingContainer"/>.</returns>
        protected virtual ScrollingContainer CreateScrollingContainer() => new LinearScrollingContainer(ControlPoint);
    }
}
