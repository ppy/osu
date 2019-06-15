// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Bindables;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A <see cref="Bindable{T}"/> for the <see cref="OsuGame"/> beatmap.
    /// This should be used sparingly in-favour of <see cref="IBindable{WorkingBeatmap}"/>.
    /// </summary>
    public abstract class BindableBeatmap : NonNullableBindable<WorkingBeatmap>
    {
        private WorkingBeatmap lastBeatmap;

        protected BindableBeatmap(WorkingBeatmap defaultValue)
            : base(defaultValue)
        {
            BindValueChanged(b => updateAudioTrack(b.NewValue), true);
        }

        private void updateAudioTrack(WorkingBeatmap beatmap)
        {
            var trackLoaded = lastBeatmap?.TrackLoaded ?? false;

            // compare to last beatmap as sometimes the two may share a track representation (optimisation, see WorkingBeatmap.TransferTo)
            if (!trackLoaded || lastBeatmap?.Track != beatmap.Track)
            {
                if (trackLoaded)
                {
                    Debug.Assert(lastBeatmap != null);
                    Debug.Assert(lastBeatmap.Track != null);

                    lastBeatmap.RecycleTrack();
                }
            }

            lastBeatmap = beatmap;
        }
    }
}
