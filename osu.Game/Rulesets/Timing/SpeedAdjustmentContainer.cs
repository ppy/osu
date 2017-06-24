// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A container for hit objects which applies applies the speed adjustments defined by the properties of a <see cref="Timing.MultiplierControlPoint"/>
    /// to affect the scroll speed of the contained <see cref="DrawableTimingSection"/>.
    ///
    /// <para>
    /// This container must always be relatively-sized to its parent to provide the speed adjustments. This container will provide the speed adjustments
    /// by modifying its size while maintaining a constant <see cref="Container{T}.RelativeChildSize"/> for its children
    /// </para>
    /// </summary>
    public abstract class SpeedAdjustmentContainer : Container<DrawableHitObject>
    {
        private readonly Bindable<double> visibleTimeRange = new Bindable<double>();
        /// <summary>
        /// Gets or sets the range of time that is visible by the length of this container.
        /// </summary>
        public Bindable<double> VisibleTimeRange
        {
            get { return visibleTimeRange; }
            set { visibleTimeRange.BindTo(value); }
        }

        protected override Container<DrawableHitObject> Content => content;
        private Container<DrawableHitObject> content;

        /// <summary>
        /// Axes which the content of this container will scroll through.
        /// </summary>
        /// <returns></returns>
        public Axes ScrollingAxes { get; internal set; }

        public readonly MultiplierControlPoint ControlPoint;

        private DrawableTimingSection timingSection;

        /// <summary>
        /// Creates a new <see cref="SpeedAdjustmentContainer"/>.
        /// </summary>
        /// <param name="controlPoint">The <see cref="MultiplierControlPoint"/> which provides the speed adjustments for this container.</param>
        protected SpeedAdjustmentContainer(MultiplierControlPoint controlPoint)
        {
            ControlPoint = controlPoint;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            timingSection = CreateTimingSection();

            timingSection.ScrollingAxes = ScrollingAxes;
            timingSection.ControlPoint = ControlPoint;
            timingSection.VisibleTimeRange.BindTo(VisibleTimeRange);
            timingSection.RelativeChildOffset = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)ControlPoint.StartTime : 0, (ScrollingAxes & Axes.Y) > 0 ? (float)ControlPoint.StartTime : 0);

            AddInternal(content = timingSection);
        }

        protected override void Update()
        {
            float multiplier = (float)ControlPoint.Multiplier;

            // The speed adjustment happens by modifying our size by the multiplier while maintaining the visible time range as the relatve size for our children
            Size = new Vector2((ScrollingAxes & Axes.X) > 0 ? multiplier : 1, (ScrollingAxes & Axes.Y) > 0 ? multiplier : 1);
            RelativeChildSize = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)VisibleTimeRange : 1, (ScrollingAxes & Axes.Y) > 0 ? (float)VisibleTimeRange : 1);
        }

        public override double LifetimeStart => ControlPoint.StartTime - VisibleTimeRange;
        public override double LifetimeEnd => ControlPoint.StartTime + timingSection.Duration + VisibleTimeRange;

        public override void Add(DrawableHitObject drawable)
        {
            var scrollingHitObject = drawable as IScrollingHitObject;
            scrollingHitObject?.LifetimeOffset.BindTo(VisibleTimeRange);

            base.Add(drawable);
        }

        /// <summary>
        /// Whether this speed adjustment can contain a hit object. This is true if the hit object occurs after this speed adjustment with respect to time.
        /// </summary>
        public bool CanContain(DrawableHitObject hitObject) => CanContain(hitObject.HitObject.StartTime);

        /// <summary>
        /// Whether this speed adjustment can contain an object placed at a time value. This is true if the time occurs after this speed adjustment.
        /// </summary>
        public bool CanContain(double startTime) => ControlPoint.StartTime <= startTime;

        /// <summary>
        /// Creates the container which handles the movement of a collection of hit objects.
        /// </summary>
        /// <returns>The <see cref="DrawableTimingSection"/>.</returns>
        protected abstract DrawableTimingSection CreateTimingSection();
    }
}