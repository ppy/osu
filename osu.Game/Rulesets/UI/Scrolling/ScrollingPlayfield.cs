// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// A type of <see cref="Playfield"/> specialized towards scrolling <see cref="DrawableHitObject"/>s.
    /// </summary>
    public abstract class ScrollingPlayfield : Playfield, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// The default span of time visible by the length of the scrolling axes.
        /// This is clamped between <see cref="time_span_min"/> and <see cref="time_span_max"/>.
        /// </summary>
        private const double time_span_default = 1500;

        /// <summary>
        /// The minimum span of time that may be visible by the length of the scrolling axes.
        /// </summary>
        private const double time_span_min = 50;

        /// <summary>
        /// The maximum span of time that may be visible by the length of the scrolling axes.
        /// </summary>
        private const double time_span_max = 10000;

        /// <summary>
        /// The step increase/decrease of the span of time visible by the length of the scrolling axes.
        /// </summary>
        private const double time_span_step = 200;

        /// <summary>
        /// The span of time that is visible by the length of the scrolling axes.
        /// For example, only hit objects with start time less than or equal to 1000 will be visible with <see cref="VisibleTimeRange"/> = 1000.
        /// </summary>
        public readonly BindableDouble VisibleTimeRange = new BindableDouble(time_span_default)
        {
            Default = time_span_default,
            MinValue = time_span_min,
            MaxValue = time_span_max
        };

        /// <summary>
        /// Whether the player can change <see cref="VisibleTimeRange"/>.
        /// </summary>
        protected virtual bool UserScrollSpeedAdjustment => true;

        /// <summary>
        /// The container that contains the <see cref="DrawableHitObject"/>s.
        /// </summary>
        public new ScrollingHitObjectContainer HitObjects => (ScrollingHitObjectContainer)HitObjectContainer;

        /// <summary>
        /// The direction in which <see cref="DrawableHitObject"/>s in this <see cref="ScrollingPlayfield"/> should scroll.
        /// </summary>
        protected readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        /// <summary>
        /// Creates a new <see cref="ScrollingPlayfield"/>.
        /// </summary>
        /// <param name="customWidth">The width to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customHeight"/> is desired. If <paramref name="customHeight"/> is also null, no scaling will occur.
        /// </param>
        /// <param name="customHeight">The height to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customWidth"/> is desired. If <paramref name="customWidth"/> is also null, no scaling will occur.
        /// </param>
        protected ScrollingPlayfield(float? customWidth = null, float? customHeight = null)
            : base(customWidth, customHeight)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HitObjects.TimeRange.BindTo(VisibleTimeRange);
        }

        public bool OnPressed(GlobalAction action)
        {
            if (!UserScrollSpeedAdjustment)
                return false;

            switch (action)
            {
                case GlobalAction.IncreaseScrollSpeed:
                    this.TransformBindableTo(VisibleTimeRange, VisibleTimeRange - time_span_step, 200, Easing.OutQuint);
                    return true;
                case GlobalAction.DecreaseScrollSpeed:
                    this.TransformBindableTo(VisibleTimeRange, VisibleTimeRange + time_span_step, 200, Easing.OutQuint);
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;

        protected sealed override HitObjectContainer CreateHitObjectContainer()
        {
            var container = new ScrollingHitObjectContainer();
            container.Direction.BindTo(Direction);
            return container;
        }
    }
}
