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

        // The lifetime, as set by the hitobject.
        private double realLifetimeStart = double.MinValue;
        private double realLifetimeEnd = double.MaxValue;

        // This method is called even if `start == LifetimeStart` when `KeepAlive` is true (necessary to update `realLifetimeStart`).
        protected override void SetLifetimeStart(double start)
        {
            realLifetimeStart = start;
            if (!keepAlive)
                base.SetLifetimeStart(start);
        }

        protected override void SetLifetimeEnd(double end)
        {
            realLifetimeEnd = end;
            if (!keepAlive)
                base.SetLifetimeEnd(end);
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
                if (keepAlive)
                    SetLifetime(double.MinValue, double.MaxValue);
                else
                    SetLifetime(realLifetimeStart, realLifetimeEnd);
            }
        }

        /// <summary>
        /// A safe offset prior to the start time of <see cref="HitObject"/> at which it may begin displaying contents.
        /// By default, <see cref="HitObject"/>s are assumed to display their contents within 10 seconds prior to their start time.
        /// </summary>
        /// <remarks>
        /// This is only used as an optimisation to delay the initial update of the <see cref="HitObject"/> and may be tuned more aggressively if required.
        /// It is indirectly used to decide the automatic transform offset provided to <see cref="DrawableHitObject.UpdateInitialTransforms"/>.
        /// A more accurate <see cref="LifetimeEntry.LifetimeStart"/> should be set for further optimisation (in <see cref="DrawableHitObject.LoadComplete"/>, for example).
        /// </remarks>
        protected virtual double InitialLifetimeOffset => 10000;

        /// <summary>
        /// Resets <see cref="LifetimeEntry.LifetimeStart"/> according to the change in start time of the <see cref="HitObject"/>.
        /// </summary>
        private void onStartTimeChanged(ValueChangedEvent<double> startTime) => LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;
    }
}
