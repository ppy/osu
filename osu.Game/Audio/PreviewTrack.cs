﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Threading;

namespace osu.Game.Audio
{
    public abstract class PreviewTrack : Component
    {
        /// <summary>
        /// Invoked when this <see cref="PreviewTrack"/> has stopped playing.
        /// </summary>
        public event Action Stopped;

        /// <summary>
        /// Invoked when this <see cref="PreviewTrack"/> has started playing.
        /// </summary>
        public event Action Started;

        private Track track;
        private bool hasStarted;

        [BackgroundDependencyLoader]
        private void load()
        {
            track = GetTrack();
        }

        /// <summary>
        /// Length of the track.
        /// </summary>
        public double Length => track?.Length ?? 0;

        /// <summary>
        /// The current track time.
        /// </summary>
        public double CurrentTime => track?.CurrentTime ?? 0;

        /// <summary>
        /// Whether the track is loaded.
        /// </summary>
        public bool TrackLoaded => track?.IsLoaded ?? false;

        /// <summary>
        /// Whether the track is playing.
        /// </summary>
        public bool IsRunning => track?.IsRunning ?? false;

        protected override void Update()
        {
            base.Update();

            // Todo: Track currently doesn't signal its completion, so we have to handle it manually
            if (hasStarted && track.HasCompleted)
                Stop();
        }

        private ScheduledDelegate startDelegate;

        /// <summary>
        /// Starts playing this <see cref="PreviewTrack"/>.
        /// </summary>
        public void Start() => startDelegate = Schedule(() =>
        {
            if (track == null)
                return;

            if (hasStarted)
                return;
            hasStarted = true;

            track.Restart();
            Started?.Invoke();
        });

        /// <summary>
        /// Stops playing this <see cref="PreviewTrack"/>.
        /// </summary>
        public void Stop()
        {
            startDelegate?.Cancel();

            if (track == null)
                return;

            if (!hasStarted)
                return;
            hasStarted = false;

            track.Stop();
            Stopped?.Invoke();
        }

        /// <summary>
        /// Retrieves the audio track.
        /// </summary>
        protected abstract Track GetTrack();
    }
}
