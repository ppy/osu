// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetInfo : IHasPrimaryKey, IHasFiles<BeatmapSetFileInfo>, ISoftDelete
    {
        public int ID { get; set; }

        private int? onlineBeatmapSetID;

        public int? OnlineBeatmapSetID
        {
            get => onlineBeatmapSetID;
            set => onlineBeatmapSetID = value > 0 ? value : null;
        }

        public BeatmapSetOnlineStatus Status { get; set; } = BeatmapSetOnlineStatus.None;

        public BeatmapMetadata Metadata { get; set; }

        public List<BeatmapInfo> Beatmaps { get; set; }

        [NotMapped]
        public BeatmapSetOnlineInfo OnlineInfo { get; set; }

        public double MaxStarDifficulty => Beatmaps?.Max(b => b.StarDifficulty) ?? 0;

        [NotMapped]
        public bool DeletePending { get; set; }

        public string Hash { get; set; }

        public string StoryboardFile => Files?.Find(f => f.Filename.EndsWith(".osb"))?.Filename;

        public List<BeatmapSetFileInfo> Files { get; set; }

        public override string ToString() => Metadata?.ToString() ?? base.ToString();

        public bool Protected { get; set; }
    }
}
