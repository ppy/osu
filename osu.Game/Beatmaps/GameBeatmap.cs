// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A <see cref="Bindable{WorkingBeatmap}"/> for the <see cref="OsuGame"/> beatmap.
    /// This should be used sparingly in-favour of <see cref="IGameBeatmap"/>.
    /// </summary>
    public abstract class GameBeatmap : NonNullableBindable<WorkingBeatmap>, IGameBeatmap
    {
        private AudioManager audioManager;
        private WorkingBeatmap lastBeatmap;

        protected GameBeatmap(WorkingBeatmap defaultValue)
            : base(defaultValue)
        {
        }

        /// <summary>
        /// Registers an <see cref="AudioManager"/> for <see cref="Track"/>s to be added to.
        /// </summary>
        /// <param name="audioManager">The <see cref="AudioManager"/> to register.</param>
        protected void RegisterAudioManager([NotNull] AudioManager audioManager)
        {
            if (this.audioManager != null) throw new InvalidOperationException($"Cannot register multiple {nameof(AudioManager)}s.");

            this.audioManager = audioManager;

            ValueChanged += registerAudioTrack;

            // If the track has changed prior to this being called, let's register it
            if (Value != Default)
                registerAudioTrack(Value);
        }

        private void registerAudioTrack(WorkingBeatmap beatmap)
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

                audioManager.Track.AddItem(beatmap.Track);
            }

            lastBeatmap = beatmap;
        }

        [NotNull]
        IGameBeatmap IGameBeatmap.GetBoundCopy() => GetBoundCopy();

        /// <summary>
        /// Retrieve a new <see cref="GameBeatmap"/> instance weakly bound to this <see cref="GameBeatmap"/>.
        /// If you are further binding to events of the retrieved <see cref="GameBeatmap"/>, ensure a local reference is held.
        /// </summary>
        [NotNull]
        public abstract GameBeatmap GetBoundCopy();
    }
}
