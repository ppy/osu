// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Timing
{
    public class TimingChangeContainer : Container<DrawableTimingChange>
    {
        /// <summary>
        /// The amount of time which this container spans.
        /// </summary>
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

        /// <summary>
        /// Finds the most applicable timing change that can contain a hit object.
        /// </summary>
        /// <param name="hitObject">The hit object to contain.</param>
        /// <returns>The last timing change which can contain <paramref name="hitObject"/>.</returns>
        private DrawableTimingChange timingChangeFor(DrawableHitObject hitObject) => Children.LastOrDefault(c => c.CanContain(hitObject)) ?? Children.FirstOrDefault();
    }
}