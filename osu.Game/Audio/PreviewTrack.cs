// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
            if (track != null)
                track.Completed += () => Schedule(Stop);
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

        private ScheduledDelegate startDelegate;

        /// <summary>
        /// Starts playing this <see cref="PreviewTrack"/>.
        /// </summary>
        /// <returns>Whether the track is started or already playing.</returns>
        public bool Start()
        {
            if (track == null)
                return false;

            startDelegate = Schedule(() =>
            {
                if (hasStarted)
                    return;

                hasStarted = true;

                track.Restart();
                Started?.Invoke();
            });

            return true;
        }

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
