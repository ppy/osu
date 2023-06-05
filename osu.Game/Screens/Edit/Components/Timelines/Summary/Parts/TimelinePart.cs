// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public partial class TimelinePart : TimelinePart<Drawable>
    {
    }

    /// <summary>
    /// Represents a part of the summary timeline..
    /// </summary>
    public partial class TimelinePart<T> : Container<T> where T : Drawable
    {
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; } = null!;

        protected readonly IBindable<Track> Track = new Bindable<Track>();

        private readonly Container<T> content;

        protected override Container<T> Content => content;

        public TimelinePart(Container<T>? content = null)
        {
            AddInternal(this.content = content ?? new Container<T> { RelativeSizeAxes = Axes.Both });

            beatmap.ValueChanged += _ =>
            {
                updateRelativeChildSize();
            };

            Track.ValueChanged += _ => updateRelativeChildSize();
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, EditorClock clock)
        {
            this.beatmap.BindTo(beatmap);
            LoadBeatmap(EditorBeatmap);

            Track.BindTo(clock.Track);
        }

        private void updateRelativeChildSize()
        {
            // If the track is not loaded, assign a default sane length otherwise relative positioning becomes meaningless.
            double trackLength = beatmap.Value.Track.IsLoaded ? beatmap.Value.Track.Length : 60000;
            content.RelativeChildSize = new Vector2((float)Math.Max(1, trackLength), 1);

            // The track may not be loaded completely (only has a length once it is).
            if (!beatmap.Value.Track.IsLoaded)
                Schedule(updateRelativeChildSize);
        }

        protected virtual void LoadBeatmap(EditorBeatmap beatmap)
        {
            content.Clear();
        }
    }
}
