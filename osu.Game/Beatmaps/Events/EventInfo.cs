// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Beatmaps.Events
{
    public class EventInfo
    {
        /// <summary>
        /// All the background events.
        /// </summary>
        public readonly List<BackgroundEvent> Backgrounds = new List<BackgroundEvent>();

        /// <summary>
        /// All the break events.
        /// </summary>
        public readonly List<BreakEvent> Breaks = new List<BreakEvent>();

        /// <summary>
        /// Total duration of all breaks.
        /// </summary>
        public double TotalBreakTime => Breaks.Sum(b => b.Duration);

        /// <summary>
        /// Retrieves the active background at a time.
        /// </summary>
        /// <param name="time">The time to retrieve the background at.</param>
        /// <returns>The background.</returns>
        public BackgroundEvent BackgroundAt(double time) => Backgrounds.FirstOrDefault(b => b.StartTime <= time);
    }
}
