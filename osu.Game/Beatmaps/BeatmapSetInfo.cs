﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetInfo : IHasPrimaryKey, IHasFiles<BeatmapSetFileInfo>, ISoftDelete
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int? OnlineBeatmapSetID { get; set; }

        public BeatmapMetadata Metadata { get; set; }

        public List<BeatmapInfo> Beatmaps { get; set; }

        [NotMapped]
        public BeatmapSetOnlineInfo OnlineInfo { get; set; }

        public double MaxStarDifficulty => Beatmaps.Max(b => b.StarDifficulty);

        [NotMapped]
        public bool DeletePending { get; set; }

        public string Hash { get; set; }

        public string StoryboardFile => Files.FirstOrDefault(f => f.Filename.EndsWith(".osb"))?.Filename;

        public List<BeatmapSetFileInfo> Files { get; set; }

        public override string ToString() => Metadata.ToString();

        public bool Protected { get; set; }
    }
}
