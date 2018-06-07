// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using OpenTK;
using osu.Framework.Configuration;
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
                LoadBeatmap(b);
            };
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap)
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

            // Todo: This should be handled more gracefully
            timeline.RelativeChildSize = Beatmap.Value.Track.Length == double.PositiveInfinity ? Vector2.One : new Vector2((float)Math.Max(1, Beatmap.Value.Track.Length), 1);
        }

        protected void Add(Drawable visualisation) => timeline.Add(visualisation);

        protected virtual void LoadBeatmap(WorkingBeatmap beatmap)
        {
            timeline.Clear();
        }
    }
}
