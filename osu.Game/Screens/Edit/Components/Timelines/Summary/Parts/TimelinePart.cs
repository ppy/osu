// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// Represents a part of the summary timeline..
    /// </summary>
    public abstract class TimelinePart : CompositeDrawable
    {
        protected readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly Container timeline;

        protected TimelinePart()
        {
            AddInternal(timeline = new Container { RelativeSizeAxes = Axes.Both });

            Beatmap.ValueChanged += b =>
            {
                updateRelativeChildSize();
                LoadBeatmap(b.NewValue);
            };
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            Beatmap.BindTo(beatmap);
        }

        private void updateRelativeChildSize()
        {
            // the track may not be loaded completely (only has a length once it is).
            if (!Beatmap.Value.Track.IsLoaded)
            {
                timeline.RelativeChildSize = Vector2.One;
                Schedule(updateRelativeChildSize);
                return;
            }

            timeline.RelativeChildSize = new Vector2((float)Math.Max(1, Beatmap.Value.Track.Length), 1);
        }

        protected void Add(Drawable visualisation) => timeline.Add(visualisation);

        protected virtual void LoadBeatmap(WorkingBeatmap beatmap)
        {
            timeline.Clear();
        }
    }
}
