// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing.Drawables;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A collection of timing sections which contain hit objects.
    /// 
    /// <para>
    /// This container provides <see cref="TimeSpan"/> for the timing sections. This is a value that indicates the amount of time
    /// that is visible throughout the span of this container.
    /// For example, only hit objects with start time less than or equal to 1000 will be visible with <see cref="TimeSpan"/> = 1000.
    /// </para>
    /// </summary>
    public class TimingSectionCollection : Container<DrawableTimingSection>
    {
        /// <summary>
        /// The length of time visible throughout the span of this container.
        /// </summary>
        public double TimeSpan;

        /// <summary>
        /// Adds a hit object to the most applicable timing section in this container.
        /// </summary>
        /// <param name="hitObject">The hit object to add.</param>
        public void Add(DrawableHitObject hitObject)
        {
            var target = timingSectionFor(hitObject);

            if (target == null)
                throw new ArgumentException("No timing section could be found that can contain the hit object.", nameof(hitObject));

            target.Add(hitObject);
        }

        protected override IComparer<Drawable> DepthComparer => new TimingSectionReverseStartTimeComparer();

        /// <summary>
        /// Finds the most applicable timing section that can contain a hit object. If the hit object occurs before the first (time-wise)
        /// timing section, then the timing section returned is the first (time-wise) timing section.
        /// </summary>
        /// <param name="hitObject">The hit object to contain.</param>
        /// <returns>The last (time-wise) timing section which can contain <paramref name="hitObject"/>. Null if no timing section exists.</returns>
        private DrawableTimingSection timingSectionFor(DrawableHitObject hitObject) => Children.FirstOrDefault(c => c.CanContain(hitObject)) ?? Children.LastOrDefault();

        /// <summary>
        /// Compares two timing sections by their start time, falling back to creation order if their start time is equal.
        /// This will compare the two timing sections in reverse order.
        /// </summary>
        private class TimingSectionReverseStartTimeComparer : ReverseCreationOrderDepthComparer
        {
            public override int Compare(Drawable x, Drawable y)
            {
                var timingChangeX = x as DrawableTimingSection;
                var timingChangeY = y as DrawableTimingSection;

                // If either of the two drawables are not hit objects, fall back to the base comparer
                if (timingChangeX?.TimingSection == null || timingChangeY?.TimingSection == null)
                    return base.Compare(x, y);

                // Compare by start time
                int i = timingChangeY.TimingSection.Time.CompareTo(timingChangeX.TimingSection.Time);

                return i != 0 ? i : base.Compare(x, y);
            }
        }
    }
}