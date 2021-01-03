// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// The local availability information about a certain beatmap for the client.
    /// </summary>
    public readonly struct BeatmapAvailability : IEquatable<BeatmapAvailability>
    {
        /// <summary>
        /// The beatmap's availability state.
        /// </summary>
        public readonly DownloadState State;

        /// <summary>
        /// The beatmap's downloading progress, null when not in <see cref="DownloadState.Downloading"/> state.
        /// </summary>
        public readonly double? DownloadProgress;

        /// <summary>
        /// Constructs a new non-<see cref="DownloadState.Downloading"/> beatmap availability state.
        /// </summary>
        /// <param name="state">The beatmap availability state.</param>
        /// <exception cref="ArgumentException">Throws if <see cref="DownloadState.Downloading"/> was specified in this constructor, as it has its own constructor (see <see cref="BeatmapAvailability(DownloadState, double)"/>.</exception>
        public BeatmapAvailability(DownloadState state)
        {
            if (state == DownloadState.Downloading)
                throw new ArgumentException($"{nameof(DownloadState.Downloading)} state has its own constructor, use it instead.");

            State = state;
            DownloadProgress = null;
        }

        /// <summary>
        /// Constructs a new <see cref="DownloadState.Downloading"/>-specific beatmap availability state.
        /// </summary>
        /// <param name="state">The beatmap availability state (always <see cref="DownloadState.Downloading"/>).</param>
        /// <param name="downloadProgress">The beatmap's downloading current progress.</param>
        /// <exception cref="ArgumentException">Throws if non-<see cref="DownloadState.Downloading"/> was specified in this constructor, as they have their own constructor (see <see cref="BeatmapAvailability(DownloadState)"/>.</exception>
        public BeatmapAvailability(DownloadState state, double downloadProgress)
        {
            if (state != DownloadState.Downloading)
                throw new ArgumentException($"This is a constructor specific for {DownloadState.Downloading} state, use the regular one instead.");

            State = DownloadState.Downloading;
            DownloadProgress = downloadProgress;
        }

        public bool Equals(BeatmapAvailability other) => State == other.State && DownloadProgress == other.DownloadProgress;

        public override string ToString() => $"{string.Join(", ", State, $"{DownloadProgress:0.00%}")}";
    }
}
