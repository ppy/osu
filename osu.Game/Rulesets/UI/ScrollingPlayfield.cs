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
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A type of <see cref="Playfield{TObject, TJudgement}"/> specialized towards scrolling <see cref="DrawableHitObject"/>s.
    /// </summary>
    public class ScrollingPlayfield<TObject, TJudgement> : Playfield<TObject, TJudgement>
        where TObject : HitObject
        where TJudgement : Judgement
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
        /// Whether to reverse the scrolling direction is reversed.
        /// </summary>
        public readonly BindableBool Reversed = new BindableBool();

        /// <summary>
        /// The container that contains the <see cref="SpeedAdjustmentContainer"/>s and <see cref="DrawableHitObject"/>s.
        /// </summary>
        internal new readonly ScrollingHitObjectContainer HitObjects;

        /// <summary>
        /// Creates a new <see cref="ScrollingPlayfield{TObject, TJudgement}"/>.
        /// </summary>
        /// <param name="scrollingAxes">The axes on which <see cref="DrawableHitObject"/>s in this container should scroll.</param>
        /// <param name="customWidth">Whether we want our internal coordinate system to be scaled to a specified width</param>
        protected ScrollingPlayfield(Axes scrollingAxes, float? customWidth = null)
            : base(customWidth)
        {
            base.HitObjects = HitObjects = new ScrollingHitObjectContainer(scrollingAxes)
            {
                RelativeSizeAxes = Axes.Both,
                VisibleTimeRange = VisibleTimeRange,
                Reversed = Reversed
            };
        }

        private List<ScrollingPlayfield<TObject, TJudgement>> nestedPlayfields;
        /// <summary>
        /// All the <see cref="ScrollingPlayfield{TObject, TJudgement}"/>s nested inside this playfield.
        /// </summary>
        public IEnumerable<ScrollingPlayfield<TObject, TJudgement>> NestedPlayfields => nestedPlayfields;

        /// <summary>
        /// Adds a <see cref="ScrollingPlayfield{TObject, TJudgement}"/> to this playfield. The nested <see cref="ScrollingPlayfield{TObject, TJudgement}"/>
        /// will be given all of the same speed adjustments as this playfield.
        /// </summary>
        /// <param name="otherPlayfield">The <see cref="ScrollingPlayfield{TObject, TJudgement}"/> to add.</param>
        protected void AddNested(ScrollingPlayfield<TObject, TJudgement> otherPlayfield)
        {
            if (nestedPlayfields == null)
                nestedPlayfields = new List<ScrollingPlayfield<TObject, TJudgement>>();

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

        private class TransformVisibleTimeRange : Transform<double, ScrollingPlayfield<TObject, TJudgement>>
        {
            private double valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            public override string TargetMember => "VisibleTimeRange.Value";

            protected override void Apply(ScrollingPlayfield<TObject, TJudgement> d, double time) => d.VisibleTimeRange.Value = valueAt(time);
            protected override void ReadIntoStartValue(ScrollingPlayfield<TObject, TJudgement> d) => StartValue = d.VisibleTimeRange.Value;
        }

        /// <summary>
        /// A container that provides the foundation for sorting <see cref="DrawableHitObject"/>s into <see cref="SpeedAdjustmentContainer"/>s.
        /// </summary>
        internal class ScrollingHitObjectContainer : HitObjectContainer<DrawableHitObject<TObject, TJudgement>>
        {
            private readonly BindableDouble visibleTimeRange = new BindableDouble { Default = 1000 };
            /// <summary>
            /// Gets or sets the range of time that is visible by the length of the scrolling axes.
            /// For example, only hit objects with start time less than or equal to 1000 will be visible with <see cref="VisibleTimeRange"/> = 1000.
            /// </summary>
            public Bindable<double> VisibleTimeRange
            {
                get { return visibleTimeRange; }
                set { visibleTimeRange.BindTo(value); }
            }

            private readonly BindableBool reversed = new BindableBool();
            /// <summary>
            /// Whether to reverse the scrolling direction is reversed.
            /// </summary>
            public BindableBool Reversed
            {
                get { return reversed; }
                set { reversed.BindTo(value); }
            }

            protected override Container<DrawableHitObject<TObject, TJudgement>> Content => content;
            private readonly Container<DrawableHitObject<TObject, TJudgement>> content;

            /// <summary>
            /// Hit objects that are to be re-processed on the next update.
            /// </summary>
            private readonly Queue<DrawableHitObject<TObject, TJudgement>> queuedHitObjects = new Queue<DrawableHitObject<TObject, TJudgement>>();

            private readonly Axes scrollingAxes;

            /// <summary>
            /// Creates a new <see cref="ScrollingHitObjectContainer"/>.
            /// </summary>
            /// <param name="scrollingAxes">The axes upon which hit objects should appear to scroll inside this container.</param>
            public ScrollingHitObjectContainer(Axes scrollingAxes)
            {
                this.scrollingAxes = scrollingAxes;

                // The following is never used - it only exists for the purpose of being able to use AddInternal below.
                content = new Container<DrawableHitObject<TObject, TJudgement>>();
            }

            /// <summary>
            /// Adds a <see cref="SpeedAdjustmentContainer"/> to this container.
            /// </summary>
            /// <param name="speedAdjustment">The <see cref="SpeedAdjustmentContainer"/>.</param>
            public void AddSpeedAdjustment(SpeedAdjustmentContainer speedAdjustment)
            {
                speedAdjustment.VisibleTimeRange.BindTo(VisibleTimeRange);
                speedAdjustment.ScrollingAxes = scrollingAxes;
                speedAdjustment.Reversed = Reversed;
                AddInternal(speedAdjustment);
            }

            /// <summary>
            /// Adds a hit object to this <see cref="ScrollingHitObjectContainer"/>. The hit objects will be queued to be processed
            /// new <see cref="SpeedAdjustmentContainer"/>s are added to this <see cref="ScrollingHitObjectContainer"/>.
            /// </summary>
            /// <param name="hitObject">The hit object to add.</param>
            public override void Add(DrawableHitObject<TObject, TJudgement> hitObject)
            {
                if (!(hitObject is IScrollingHitObject))
                    throw new InvalidOperationException($"Hit objects added to a {nameof(ScrollingHitObjectContainer)} must implement {nameof(IScrollingHitObject)}.");

                queuedHitObjects.Enqueue(hitObject);
            }

            public override bool Remove(DrawableHitObject<TObject, TJudgement> hitObject)
            {
                foreach (var c in InternalChildren.OfType<SpeedAdjustmentContainer>())
                    c.Remove(hitObject);
                return true;
            }

            protected override void Update()
            {
                base.Update();

                // Todo: At the moment this is going to re-process every single Update, however this will only be a null-op
                // when there are no SpeedAdjustmentContainers available. This should probably error or something, but it's okay for now.

                // An external count is kept because hit objects that can't be added are re-queued
                int count = queuedHitObjects.Count;
                while (count-- > 0)
                {
                    var hitObject = queuedHitObjects.Dequeue();

                    var target = adjustmentContainerFor(hitObject);
                    if (target == null)
                    {
                        // We can't add this hit object to a speed adjustment container yet, so re-queue it
                        // for re-processing when the layout next invalidated
                        queuedHitObjects.Enqueue(hitObject);
                        continue;
                    }

                    target.Add(hitObject);
                }
            }

            /// <summary>
            /// Finds the <see cref="SpeedAdjustmentContainer"/> which provides the speed adjustment active at the start time
            /// of a hit object. If there is no <see cref="SpeedAdjustmentContainer"/> active at the start time of the hit object,
            /// then the first (time-wise) speed adjustment is returned.
            /// </summary>
            /// <param name="hitObject">The hit object to find the active <see cref="SpeedAdjustmentContainer"/> for.</param>
            /// <returns>The <see cref="SpeedAdjustmentContainer"/> active at <paramref name="hitObject"/>'s start time. Null if there are no speed adjustments.</returns>
            private SpeedAdjustmentContainer adjustmentContainerFor(DrawableHitObject hitObject) => InternalChildren.OfType<SpeedAdjustmentContainer>().FirstOrDefault(c => c.CanContain(hitObject)) ?? InternalChildren.OfType<SpeedAdjustmentContainer>().LastOrDefault();

            /// <summary>
            /// Finds the <see cref="SpeedAdjustmentContainer"/> which provides the speed adjustment active at a time.
            /// If there is no <see cref="SpeedAdjustmentContainer"/> active at the time, then the first (time-wise) speed adjustment is returned.
            /// </summary>
            /// <param name="time">The time to find the active <see cref="SpeedAdjustmentContainer"/> at.</param>
            /// <returns>The <see cref="SpeedAdjustmentContainer"/> active at <paramref name="time"/>. Null if there are no speed adjustments.</returns>
            private SpeedAdjustmentContainer adjustmentContainerAt(double time) => InternalChildren.OfType<SpeedAdjustmentContainer>().FirstOrDefault(c => c.CanContain(time)) ?? InternalChildren.OfType<SpeedAdjustmentContainer>().LastOrDefault();
        }
    }
}