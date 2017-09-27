// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
    internal abstract class TimelinePart : CompositeDrawable
    {
        public Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

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
