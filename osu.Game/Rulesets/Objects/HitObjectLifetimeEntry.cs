// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// A <see cref="LifetimeEntry"/> that stores the lifetime for a <see cref="HitObject"/>.
    /// </summary>
    public class HitObjectLifetimeEntry : LifetimeEntry
    {
        /// <summary>
        /// The <see cref="HitObject"/>.
        /// </summary>
        public readonly HitObject HitObject;

        /// <summary>
        /// The result that <see cref="HitObject"/> was judged with.
        /// This is set by the accompanying <see cref="DrawableHitObject"/>, and reused when required for rewinding.
        /// </summary>
        internal JudgementResult Result;

        private readonly IBindable<double> startTimeBindable = new BindableDouble();

        /// <summary>
        /// Creates a new <see cref="HitObjectLifetimeEntry"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to store the lifetime of.</param>
        public HitObjectLifetimeEntry(HitObject hitObject)
        {
            HitObject = hitObject;

            startTimeBindable.BindTo(HitObject.StartTimeBindable);
            startTimeBindable.BindValueChanged(onStartTimeChanged, true);
        }

        // The lifetime start, as set by the hitobject.
        private double realLifetimeStart = double.MinValue;

        /// <summary>
        /// The time at which the <see cref="HitObject"/> should become alive.
        /// </summary>
        public new double LifetimeStart
        {
            get => realLifetimeStart;
            set => setLifetime(realLifetimeStart = value, LifetimeEnd);
        }

        // The lifetime end, as set by the hitobject.
        private double realLifetimeEnd = double.MaxValue;

        /// <summary>
        /// The time at which the <see cref="HitObject"/> should become dead.
        /// </summary>
        public new double LifetimeEnd
        {
            get => realLifetimeEnd;
            set => setLifetime(LifetimeStart, realLifetimeEnd = value);
        }

        private void setLifetime(double start, double end)
        {
            if (keepAlive)
            {
                start = double.MinValue;
                end = double.MaxValue;
            }

            base.LifetimeStart = start;
            base.LifetimeEnd = end;
        }

        private bool keepAlive;

        /// <summary>
        /// Whether the <see cref="HitObject"/> should be kept always alive.
        /// </summary>
        internal bool KeepAlive
        {
            set
            {
                if (keepAlive == value)
                    return;

                keepAlive = value;
                setLifetime(realLifetimeStart, realLifetimeEnd);
            }
        }

        /// <summary>
        /// A safe offset prior to the start time of <see cref="HitObject"/> at which it may begin displaying contents.
        /// By default, <see cref="HitObject"/>s are assumed to display their contents within 10 seconds prior to their start time.
        /// </summary>
        /// <remarks>
        /// This is only used as an optimisation to delay the initial update of the <see cref="HitObject"/> and may be tuned more aggressively if required.
        /// It is indirectly used to decide the automatic transform offset provided to <see cref="DrawableHitObject.UpdateInitialTransforms"/>.
        /// A more accurate <see cref="LifetimeStart"/> should be set for further optimisation (in <see cref="DrawableHitObject.LoadComplete"/>, for example).
        /// </remarks>
        protected virtual double InitialLifetimeOffset => 10000;

        /// <summary>
        /// Resets <see cref="LifetimeStart"/> according to the change in start time of the <see cref="HitObject"/>.
        /// </summary>
        private void onStartTimeChanged(ValueChangedEvent<double> startTime) => LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;
    }
}
