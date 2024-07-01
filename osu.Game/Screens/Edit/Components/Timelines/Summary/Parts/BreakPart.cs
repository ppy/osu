// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays breaks in the song.
    /// </summary>
    public partial class BreakPart : TimelinePart
    {
        private readonly BindableList<BreakPeriod> breaks = new BindableList<BreakPeriod>();

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            breaks.UnbindAll();
            breaks.BindTo(beatmap.Breaks);
            breaks.BindCollectionChanged((_, _) =>
            {
                Clear();
                foreach (var breakPeriod in beatmap.Breaks)
                    Add(new BreakVisualisation(breakPeriod));
            }, true);
        }

        private partial class BreakVisualisation : Circle
        {
            public BreakVisualisation(BreakPeriod breakPeriod)
            {
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Both;
                X = (float)breakPeriod.StartTime;
                Width = (float)breakPeriod.Duration;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Gray7;
        }
    }
}
