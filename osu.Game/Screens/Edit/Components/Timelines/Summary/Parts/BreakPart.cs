// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays breaks in the song.
    /// </summary>
    public class BreakPart : TimelinePart
    {
        protected override void LoadBeatmap(WorkingBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);
            foreach (var breakPeriod in beatmap.Beatmap.Breaks)
                Add(new BreakVisualisation(breakPeriod));
        }

        private class BreakVisualisation : DurationVisualisation
        {
            public BreakVisualisation(BreakPeriod breakPeriod)
                : base(breakPeriod.StartTime, breakPeriod.EndTime)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Yellow;
        }
    }
}
