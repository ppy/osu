// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Threading;

namespace osu.Game.Audio
{
    [LongRunningLoad]
    public abstract partial class PreviewTrack : Component
    {
        /// <summary>
        /// Invoked when this <see cref="PreviewTrack"/> has stopped playing.
        /// Not invoked in a thread-safe context.
        /// </summary>
        public event Action? Stopped;

        /// <summary>
        /// Invoked when this <see cref="PreviewTrack"/> has started playing.
        /// Not invoked in a thread-safe context.
        /// </summary>
        public event Action? Started;

        protected Track? Track { get; private set; }

        private bool hasStarted;

        [BackgroundDependencyLoader]
        private void load()
        {
            Track = GetTrack();
            if (Track != null)
                Track.Completed += Stop;
        }

        /// <summary>
        /// Length of the track.
        /// </summary>
        public double Length => Track?.Length ?? 0;

        /// <summary>
        /// The current track time.
        /// </summary>
        public double CurrentTime => Track?.CurrentTime ?? 0;

        /// <summary>
        /// Whether the track is loaded.
        /// </summary>
        public bool TrackLoaded => Track?.IsLoaded ?? false;

        /// <summary>
        /// Whether the track is playing.
        /// </summary>
        public bool IsRunning => Track?.IsRunning ?? false;

        private ScheduledDelegate? startDelegate;

        /// <summary>
        /// Starts playing this <see cref="PreviewTrack"/>.
        /// </summary>
        /// <returns>Whether the track is started or already playing.</returns>
        public bool Start()
        {
            if (Track == null)
                return false;

            startDelegate = Schedule(() =>
            {
                if (hasStarted)
                    return;

                hasStarted = true;

                Track.Restart();
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

            if (Track == null)
                return;

            if (!hasStarted)
                return;

            hasStarted = false;

            Track.Stop();

            // Ensure the track is reset immediately on stopping, so the next time it is started it has a correct time value.
            Track.Seek(0);

            Stopped?.Invoke();
        }

        /// <summary>
        /// Retrieves the audio track.
        /// </summary>
        protected abstract Track? GetTrack();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Stop();
            Track?.Dispose();

            Track = null;
        }
    }
}
