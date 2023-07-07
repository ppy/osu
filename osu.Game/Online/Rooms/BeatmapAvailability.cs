// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using MessagePack;
using Newtonsoft.Json;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// The local availability information about a certain beatmap for the client.
    /// </summary>
    [MessagePackObject]
    public class BeatmapAvailability : IEquatable<BeatmapAvailability>
    {
        /// <summary>
        /// The beatmap's availability state.
        /// </summary>
        [Key(0)]
        public readonly DownloadState State;

        /// <summary>
        /// The beatmap's downloading progress, null when not in <see cref="DownloadState.Downloading"/> state.
        /// </summary>
        [Key(1)]
        public readonly float? DownloadProgress;

        [JsonConstructor]
        public BeatmapAvailability(DownloadState state, float? downloadProgress = null)
        {
            State = state;
            DownloadProgress = downloadProgress;
        }

        public static BeatmapAvailability Unknown() => new BeatmapAvailability(DownloadState.Unknown);
        public static BeatmapAvailability NotDownloaded() => new BeatmapAvailability(DownloadState.NotDownloaded);
        public static BeatmapAvailability Downloading(float progress) => new BeatmapAvailability(DownloadState.Downloading, progress);
        public static BeatmapAvailability Importing() => new BeatmapAvailability(DownloadState.Importing);
        public static BeatmapAvailability LocallyAvailable() => new BeatmapAvailability(DownloadState.LocallyAvailable);

        public bool Equals(BeatmapAvailability other) => other != null && State == other.State && DownloadProgress == other.DownloadProgress;

        public override string ToString() => $"{string.Join(", ", State, $"{DownloadProgress:0.00%}")}";
    }
}
