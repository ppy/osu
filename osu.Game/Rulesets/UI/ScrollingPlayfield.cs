// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Input;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A type of <see cref="Playfield"/> specialized towards scrolling <see cref="DrawableHitObject"/>s.
    /// </summary>
    public class ScrollingPlayfield : Playfield
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
        /// Whether to reverse the scrolling direction is reversed. Note that this does _not_ invert the hit objects.
        /// </summary>
        protected readonly BindableBool Reversed = new BindableBool();

        /// <summary>
        /// The container that contains the <see cref="SpeedAdjustmentContainer"/>s and <see cref="DrawableHitObject"/>s.
        /// </summary>
        public new readonly ScrollingHitObjectContainer HitObjects;

        /// <summary>
        /// Creates a new <see cref="ScrollingPlayfield"/>.
        /// </summary>
        /// <param name="scrollingAxes">The axes on which <see cref="DrawableHitObject"/>s in this container should scroll.</param>
        /// <param name="customWidth">Whether we want our internal coordinate system to be scaled to a specified width</param>
        protected ScrollingPlayfield(Axes scrollingAxes, float? customWidth = null)
            : base(customWidth)
        {
            base.HitObjects = HitObjects = new ScrollingHitObjectContainer(scrollingAxes) { RelativeSizeAxes = Axes.Both };
            HitObjects.VisibleTimeRange.BindTo(VisibleTimeRange);
            HitObjects.Reversed.BindTo(Reversed);
        }

        private List<ScrollingPlayfield> nestedPlayfields;
        /// <summary>
        /// All the <see cref="ScrollingPlayfield"/>s nested inside this playfield.
        /// </summary>
        public IEnumerable<ScrollingPlayfield> NestedPlayfields => nestedPlayfields;

        /// <summary>
        /// Adds a <see cref="ScrollingPlayfield"/> to this playfield. The nested <see cref="ScrollingPlayfield"/>
        /// will be given all of the same speed adjustments as this playfield.
        /// </summary>
        /// <param name="otherPlayfield">The <see cref="ScrollingPlayfield"/> to add.</param>
        protected void AddNested(ScrollingPlayfield otherPlayfield)
        {
            if (nestedPlayfields == null)
                nestedPlayfields = new List<ScrollingPlayfield>();

            nestedPlayfields.Add(otherPlayfield);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
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

        /// <summary>
        /// A container that provides the foundation for sorting <see cref="DrawableHitObject"/>s into <see cref="SpeedAdjustmentContainer"/>s.
        /// </summary>
        public class ScrollingHitObjectContainer : HitObjectContainer
        {
            /// <summary>
            /// Gets or sets the range of time that is visible by the length of the scrolling axes.
            /// For example, only hit objects with start time less than or equal to 1000 will be visible with <see cref="VisibleTimeRange"/> = 1000.
            /// </summary>
            public readonly BindableDouble VisibleTimeRange = new BindableDouble { Default = 1000 };

            /// <summary>
            /// Whether to reverse the scrolling direction is reversed.
            /// </summary>
            public readonly BindableBool Reversed = new BindableBool();

            private readonly SortedContainer speedAdjustments;
            public IReadOnlyList<SpeedAdjustmentContainer> SpeedAdjustments => speedAdjustments;

            private readonly SpeedAdjustmentContainer defaultSpeedAdjustment;

            private readonly Axes scrollingAxes;

            /// <summary>
            /// Creates a new <see cref="ScrollingHitObjectContainer"/>.
            /// </summary>
            /// <param name="scrollingAxes">The axes upon which hit objects should appear to scroll inside this container.</param>
            public ScrollingHitObjectContainer(Axes scrollingAxes)
            {
                this.scrollingAxes = scrollingAxes;

                AddInternal(speedAdjustments = new SortedContainer { RelativeSizeAxes = Axes.Both });

                // Default speed adjustment
                AddSpeedAdjustment(defaultSpeedAdjustment = new SpeedAdjustmentContainer(new MultiplierControlPoint(0)));
            }

            /// <summary>
            /// Adds a <see cref="SpeedAdjustmentContainer"/> to this container, re-sorting all hit objects
            /// in the last <see cref="SpeedAdjustmentContainer"/> that occurred (time-wise) before it.
            /// </summary>
            /// <param name="speedAdjustment">The <see cref="SpeedAdjustmentContainer"/>.</param>
            public void AddSpeedAdjustment(SpeedAdjustmentContainer speedAdjustment)
            {
                speedAdjustment.ScrollingAxes = scrollingAxes;
                speedAdjustment.VisibleTimeRange.BindTo(VisibleTimeRange);
                speedAdjustment.Reversed.BindTo(Reversed);

                if (speedAdjustments.Count > 0)
                {
                    // We need to re-sort all hit objects in the speed adjustment container prior to figure out if they
                    // should now lie within this one
                    var existingAdjustment = adjustmentContainerAt(speedAdjustment.ControlPoint.StartTime);
                    for (int i = 0; i < existingAdjustment.Count; i++)
                    {
                        DrawableHitObject hitObject = existingAdjustment[i];

                        if (!speedAdjustment.CanContain(hitObject.HitObject.StartTime))
                            continue;

                        existingAdjustment.Remove(hitObject);
                        speedAdjustment.Add(hitObject);

                        i--;
                    }
                }

                speedAdjustments.Add(speedAdjustment);
            }

            /// <summary>
            /// Removes a <see cref="SpeedAdjustmentContainer"/> from this container, re-sorting all hit objects
            /// which it contained into new <see cref="SpeedAdjustmentContainer"/>s.
            /// </summary>
            /// <param name="speedAdjustment">The <see cref="SpeedAdjustmentContainer"/> to remove.</param>
            public void RemoveSpeedAdjustment(SpeedAdjustmentContainer speedAdjustment)
            {
                if (speedAdjustment == defaultSpeedAdjustment)
                    throw new InvalidOperationException($"The default {nameof(SpeedAdjustmentContainer)} must not be removed.");

                if (!speedAdjustments.Remove(speedAdjustment))
                    return;

                while (speedAdjustment.Count > 0)
                {
                    DrawableHitObject hitObject = speedAdjustment[0];

                    speedAdjustment.Remove(hitObject);
                    Add(hitObject);
                }
            }

            public override IEnumerable<DrawableHitObject> Objects => speedAdjustments.SelectMany(s => s.Children);

            /// <summary>
            /// Adds a hit object to this <see cref="ScrollingHitObjectContainer"/>. The hit objects will be queued to be processed
            /// new <see cref="SpeedAdjustmentContainer"/>s are added to this <see cref="ScrollingHitObjectContainer"/>.
            /// </summary>
            /// <param name="hitObject">The hit object to add.</param>
            public override void Add(DrawableHitObject hitObject)
            {
                if (!(hitObject is IScrollingHitObject))
                    throw new InvalidOperationException($"Hit objects added to a {nameof(ScrollingHitObjectContainer)} must implement {nameof(IScrollingHitObject)}.");

                adjustmentContainerAt(hitObject.HitObject.StartTime).Add(hitObject);
            }

            public override bool Remove(DrawableHitObject hitObject) => speedAdjustments.Any(s => s.Remove(hitObject));

            /// <summary>
            /// Finds the <see cref="SpeedAdjustmentContainer"/> which provides the speed adjustment active at a time.
            /// If there is no <see cref="SpeedAdjustmentContainer"/> active at the time, then the first (time-wise) speed adjustment is returned.
            /// </summary>
            /// <param name="time">The time to find the active <see cref="SpeedAdjustmentContainer"/> at.</param>
            /// <returns>The <see cref="SpeedAdjustmentContainer"/> active at <paramref name="time"/>. Null if there are no speed adjustments.</returns>
            private SpeedAdjustmentContainer adjustmentContainerAt(double time) => speedAdjustments.FirstOrDefault(c => c.CanContain(time)) ?? defaultSpeedAdjustment;

            private class SortedContainer : Container<SpeedAdjustmentContainer>
            {
                protected override int Compare(Drawable x, Drawable y)
                {
                    var sX = (SpeedAdjustmentContainer)x;
                    var sY = (SpeedAdjustmentContainer)y;

                    int result = sY.ControlPoint.StartTime.CompareTo(sX.ControlPoint.StartTime);
                    if (result != 0)
                        return result;
                    return base.Compare(y, x);
                }
            }
        }
    }
}
