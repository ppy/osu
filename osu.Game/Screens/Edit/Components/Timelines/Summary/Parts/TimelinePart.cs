// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Framework.Allocation;
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
        private readonly Container timeline;

        protected TimelinePart()
        {
            AddInternal(timeline = new Container { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            osuGame.Beatmap.ValueChanged += b =>
            {
                timeline.Clear();
                timeline.RelativeChildSize = new Vector2((float)Math.Max(1, b.Track.Length), 1);
                LoadBeatmap(b);
            };

            timeline.RelativeChildSize = new Vector2((float)Math.Max(1, osuGame.Beatmap.Value.Track.Length), 1);
            LoadBeatmap(osuGame.Beatmap);
        }

        protected void Add(Drawable visualisation) => timeline.Add(visualisation);

        protected abstract void LoadBeatmap(WorkingBeatmap beatmap);
    }
}
