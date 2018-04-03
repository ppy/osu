// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK.Input;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// A type of <see cref="Playfield"/> specialized towards scrolling <see cref="DrawableHitObject"/>s.
    /// </summary>
    public abstract class ScrollingPlayfield : Playfield
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
        private const double time_span_step = 50;

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
        public new ScrollingHitObjectContainer HitObjects => (ScrollingHitObjectContainer)base.HitObjects;

        private readonly ScrollingDirection direction;

        /// <summary>
        /// Creates a new <see cref="ScrollingPlayfield"/>.
        /// </summary>
        /// <param name="direction">The direction in which <see cref="DrawableHitObject"/>s in this container should scroll.</param>
        /// <param name="customWidth">The width to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customHeight"/> is desired. If <paramref name="customHeight"/> is also null, no scaling will occur.
        /// </param>
        /// <param name="customHeight">The height to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customWidth"/> is desired. If <paramref name="customWidth"/> is also null, no scaling will occur.
        /// </param>
        protected ScrollingPlayfield(ScrollingDirection direction, float? customWidth = null, float? customHeight = null)
            : base(customWidth, customHeight)
        {
            this.direction = direction;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HitObjects.TimeRange.BindTo(VisibleTimeRange);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!UserScrollSpeedAdjustment)
                return false;

            if (state.Keyboard.ControlPressed)
            {
                switch (args.Key)
                {
                    case Key.Minus:
                        transformVisibleTimeRangeTo(VisibleTimeRange + time_span_step, 200, Easing.OutQuint);
                        break;
                    case Key.Plus:
                        transformVisibleTimeRangeTo(VisibleTimeRange - time_span_step, 200, Easing.OutQuint);
                        break;
                }
            }

            return false;
        }

        private void transformVisibleTimeRangeTo(double newTimeRange, double duration = 0, Easing easing = Easing.None)
        {
            this.TransformTo(this.PopulateTransform(new TransformVisibleTimeRange(), newTimeRange, duration, easing));
        }

        protected sealed override HitObjectContainer CreateHitObjectContainer() => new ScrollingHitObjectContainer(direction);

        private class TransformVisibleTimeRange : Transform<double, ScrollingPlayfield>
        {
            private double valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            public override string TargetMember => "VisibleTimeRange.Value";

            protected override void Apply(ScrollingPlayfield d, double time) => d.VisibleTimeRange.Value = valueAt(time);
            protected override void ReadIntoStartValue(ScrollingPlayfield d) => StartValue = d.VisibleTimeRange.Value;
        }
    }
}
