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
    public class TimingChangeContainer : Container<DrawableTimingChange>, IHasTimeSpan
    {
        public double TimeSpan { get; set; }

        /// <summary>
        /// Adds a hit object to the most applicable timing change in this container.
        /// </summary>
        /// <param name="hitObject">The hit object to add.</param>
        public void Add(DrawableHitObject hitObject)
        {
            var target = timingChangeFor(hitObject);

            if (target == null)
                throw new ArgumentException("No timing change could be found that can contain the hit object.", nameof(hitObject));

            target.Add(hitObject);
        }

        protected override IComparer<Drawable> DepthComparer => new TimingChangeReverseStartTimeComparer();

        /// <summary>
        /// Finds the most applicable timing change that can contain a hit object. If the hit object occurs before the first (time-wise)
        /// timing change, then the timing change returned is the first (time-wise) timing change.
        /// </summary>
        /// <param name="hitObject">The hit object to contain.</param>
        /// <returns>The last (time-wise) timing change which can contain <paramref name="hitObject"/>. Null if no timing change exists.</returns>
        private DrawableTimingChange timingChangeFor(DrawableHitObject hitObject) => Children.FirstOrDefault(c => c.CanContain(hitObject)) ?? Children.LastOrDefault();
    }

    /// <summary>
    /// Compares two timing changes by their start time, falling back to creation order if their start time is equal.
    /// This will compare the two timing changes in reverse order.
    /// </summary>
    public class TimingChangeReverseStartTimeComparer : Drawable.ReverseCreationOrderDepthComparer
    {
        public override int Compare(Drawable x, Drawable y)
        {
            var timingChangeX = x as DrawableTimingChange;
            var timingChangeY = y as DrawableTimingChange;

            // If either of the two drawables are not hit objects, fall back to the base comparer
            if (timingChangeX?.TimingChange == null || timingChangeY?.TimingChange == null)
                return base.Compare(x, y);

            // Compare by start time
            int i = timingChangeY.TimingChange.Time.CompareTo(timingChangeX.TimingChange.Time);
            if (i != 0)
                return i;

            return base.Compare(x, y);
        }
    }
}