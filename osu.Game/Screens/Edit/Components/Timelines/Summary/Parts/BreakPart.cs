// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Timing;
using osu.Game.Extensions;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays breaks in the song.
    /// </summary>
    public partial class BreakPart : TimelinePart
    {
        private readonly BindableList<BreakPeriod> breaks = new BindableList<BreakPeriod>();

        private DrawablePool<BreakVisualisation> pool = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pool = new DrawablePool<BreakVisualisation>(10));
        }

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            breaks.UnbindAll();
            breaks.BindTo(beatmap.Breaks);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            breaks.BindCollectionChanged((_, _) =>
            {
                Clear(disposeChildren: false);
                foreach (var breakPeriod in breaks)
                    Add(pool.Get(v => v.BreakPeriod = breakPeriod));
            }, true);
        }

        private partial class BreakVisualisation : PoolableDrawable, IHasTooltip
        {
            private BreakPeriod breakPeriod = null!;

            public BreakPeriod BreakPeriod
            {
                set
                {
                    breakPeriod = value;
                    X = (float)value.StartTime;
                    Width = (float)value.Duration;
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Box { RelativeSizeAxes = Axes.Both };
                Colour = colours.Gray5;
                Alpha = 0.4f;
            }

            public LocalisableString TooltipText => $"{breakPeriod.StartTime.ToEditorFormattedString()} - {breakPeriod.EndTime.ToEditorFormattedString()} break time";
        }
    }
}
