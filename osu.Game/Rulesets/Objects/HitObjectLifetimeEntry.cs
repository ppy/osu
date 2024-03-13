// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// A <see cref="LifetimeEntry"/> that stores the lifetime for a <see cref="HitObject"/>.
    /// </summary>
    public class HitObjectLifetimeEntry : LifetimeEntry<HitObjectLifetimeEntry>
    {
        /// <summary>
        /// The <see cref="HitObject"/>.
        /// </summary>
        public readonly HitObject HitObject;

        /// <summary>
        /// The list of <see cref="HitObjectLifetimeEntry"/> for the <see cref="HitObject"/>'s nested objects (if any).
        /// </summary>
        public List<HitObjectLifetimeEntry> NestedEntries { get; internal set; } = new List<HitObjectLifetimeEntry>();

        /// <summary>
        /// The result that <see cref="HitObject"/> was judged with.
        /// This is set by the accompanying <see cref="DrawableHitObject"/>, and reused when required for rewinding.
        /// </summary>
        internal JudgementResult? Result;

        /// <summary>
        /// Whether <see cref="HitObject"/> has been judged.
        /// Note: This does NOT include nested hitobjects.
        /// </summary>
        public bool Judged => Result?.HasResult ?? false;

        /// <summary>
        /// Whether <see cref="HitObject"/> and all of its nested objects have been judged.
        /// </summary>
        public bool AllJudged
        {
            get
            {
                if (!Judged)
                    return false;

                foreach (var entry in NestedEntries)
                {
                    if (!entry.AllJudged)
                        return false;
                }

                return true;
            }
        }

        private readonly IBindable<double> startTimeBindable = new BindableDouble();

        internal event Action? RevertResult;

        /// <summary>
        /// Creates a new <see cref="HitObjectLifetimeEntry"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to store the lifetime of.</param>
        public HitObjectLifetimeEntry(HitObject hitObject)
        {
            HitObject = hitObject;

            startTimeBindable.BindTo(HitObject.StartTimeBindable);
            startTimeBindable.BindValueChanged(_ => SetInitialLifetime(), true);

            // Subscribe to this event before the DrawableHitObject so that the local callback is invoked before the entry is re-applied as a result of DefaultsApplied.
            // This way, the DrawableHitObject can use OnApply() to overwrite the LifetimeStart that was set inside setInitialLifetime().
            HitObject.DefaultsApplied += _ => SetInitialLifetime();
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
        /// This is only used as an optimisation to delay the initial application of the <see cref="HitObject"/> to a <see cref="DrawableHitObject"/>.
        /// A more accurate <see cref="LifetimeEntry{T}.LifetimeStart"/> should be set on the hit object application, for further optimisation.
        /// </remarks>
        protected virtual double InitialLifetimeOffset => 10000;

        /// <summary>
        /// Set <see cref="LifetimeEntry{T}.LifetimeStart"/> using <see cref="InitialLifetimeOffset"/>.
        /// </summary>
        internal void SetInitialLifetime() => LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;

        internal void OnRevertResult() => RevertResult?.Invoke();
    }
}
