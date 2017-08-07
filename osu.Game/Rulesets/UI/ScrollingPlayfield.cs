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
    public class ScrollingPlayfield<TObject, TJudgement> : Playfield<TObject, TJudgement>
        where TObject : HitObject
        where TJudgement : Judgement
    {
        private const double time_span_default = 1500;
        private const double time_span_min = 50;
        private const double time_span_max = 10000;
        private const double time_span_step = 50;

        /// <summary>
        /// Gets or sets the range of time that is visible by the length of this playfield the scrolling axis direction.
        /// For example, only hit objects with start time less than or equal to 1000 will be visible with <see cref="VisibleTimeRange"/> = 1000.
        /// </summary>
        private readonly BindableDouble visibleTimeRange = new BindableDouble(time_span_default)
        {
            Default = time_span_default,
            MinValue = time_span_min,
            MaxValue = time_span_max
        };

        public BindableDouble VisibleTimeRange
        {
            get { return visibleTimeRange; }
            set { visibleTimeRange.BindTo(value); }
        }

        public new readonly ScrollingHitObjectContainer HitObjects;

        protected ScrollingPlayfield(Axes scrollingAxes, float? customWidth = null)
            : base(customWidth)
        {
            base.HitObjects = HitObjects = new ScrollingHitObjectContainer(scrollingAxes)
            {
                RelativeSizeAxes = Axes.Both,
                VisibleTimeRange = VisibleTimeRange
            };
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
        /// A collection of <see cref="SpeedAdjustmentContainer"/>s.
        ///
        /// <para>
        /// This container redirects any <see cref="DrawableHitObject"/>'s added to it to the <see cref="SpeedAdjustmentContainer"/>
        /// which provides the speed adjustment active at the start time of the hit object. Furthermore, this container provides the
        /// necessary <see cref="VisibleTimeRange"/> for the contained <see cref="SpeedAdjustmentContainer"/>s.
        /// </para>
        /// </summary>
        public class ScrollingHitObjectContainer : HitObjectContainer<DrawableHitObject<TObject, TJudgement>>
        {
            private readonly BindableDouble visibleTimeRange = new BindableDouble { Default = 1000 };
            /// <summary>
            /// Gets or sets the range of time that is visible by the length of this container.
            /// For example, only hit objects with start time less than or equal to 1000 will be visible with <see cref="VisibleTimeRange"/> = 1000.
            /// </summary>
            public Bindable<double> VisibleTimeRange
            {
                get { return visibleTimeRange; }
                set { visibleTimeRange.BindTo(value); }
            }

            protected override Container<DrawableHitObject<TObject, TJudgement>> Content => content;
            /// <summary>
            /// The following is never used - it only exists for the purpose of being able to use AddInternal below.
            /// </summary>
            private Container<DrawableHitObject<TObject, TJudgement>> content;

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

                content = new Container<DrawableHitObject<TObject, TJudgement>>();
            }

            public void AddSpeedAdjustment(SpeedAdjustmentContainer speedAdjustment)
            {
                speedAdjustment.VisibleTimeRange.BindTo(VisibleTimeRange);
                speedAdjustment.ScrollingAxes = scrollingAxes;
                AddInternal(speedAdjustment);
            }

            /// <summary>
            /// Adds a hit object to this <see cref="ScrollingHitObjectContainer"/>. The hit objects will be kept in a queue
            /// and will be processed when new <see cref="SpeedAdjustmentContainer"/>s are added to this <see cref="ScrollingHitObjectContainer"/>.
            /// </summary>
            /// <param name="hitObject">The hit object to add.</param>
            public override void Add(DrawableHitObject<TObject, TJudgement> hitObject)
            {
                if (!(hitObject is IScrollingHitObject))
                    throw new InvalidOperationException($"Hit objects added to a {nameof(ScrollingHitObjectContainer)} must implement {nameof(IScrollingHitObject)}.");

                queuedHitObjects.Enqueue(hitObject);
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

                    if (hitObject.RelativePositionAxes != target.ScrollingAxes)
                        throw new InvalidOperationException($"Make sure to set all {nameof(DrawableHitObject)}'s {nameof(RelativePositionAxes)} are equal to the correct axes of scrolling ({target.ScrollingAxes}).");

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