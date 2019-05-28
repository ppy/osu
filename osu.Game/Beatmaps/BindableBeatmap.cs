// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Bindables;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A <see cref="Bindable{T}"/> for the <see cref="OsuGame"/> beatmap.
    /// This should be used sparingly in-favour of <see cref="IBindable{WorkingBeatmap}"/>.
    /// </summary>
    public abstract class BindableBeatmap : NonNullableBindable<WorkingBeatmap>
    {
        protected AudioManager AudioManager;

        private WorkingBeatmap lastBeatmap;

        protected BindableBeatmap(WorkingBeatmap defaultValue, AudioManager audioManager)
            : base(defaultValue)
        {
            // we don't want to attempt to update tracks if we are a bound copy.
            if (audioManager != null)
            {
                AudioManager = audioManager;
                ValueChanged += b => updateAudioTrack(b.NewValue);

                // If the track has changed prior to this being called, let's register it
                if (Value != Default)
                    updateAudioTrack(Value);
            }
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

        /// <summary>
        /// Retrieve a new <see cref="BindableBeatmap"/> instance weakly bound to this <see cref="BindableBeatmap"/>.
        /// If you are further binding to events of the retrieved <see cref="BindableBeatmap"/>, ensure a local reference is held.
        /// </summary>
        [NotNull]
        public new abstract BindableBeatmap GetBoundCopy();
    }
}
