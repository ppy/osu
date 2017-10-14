// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetInfo : IPopulate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [NotMapped]
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

        public bool Protected { get; set; }

        public void Populate(OsuDbContext connection)
        {
            var entry = connection.Entry(this);
            entry.Collection<BeatmapInfo>(nameof(Beatmaps)).Load();
            entry.Reference<BeatmapMetadata>(nameof(Metadata)).Load();
            entry.Collection<BeatmapSetFileInfo>(nameof(Files)).Load();

            foreach (var beatmap in Beatmaps)
            {
                var beatmapEntry = connection.Entry(beatmap);
                beatmapEntry.Reference<BeatmapDifficulty>(nameof(beatmap.Difficulty)).Load();
            }

            foreach (var file in Files)
            {
                var fileEntry = connection.Entry(file);
                fileEntry.Reference<FileInfo>(nameof(file.FileInfo)).Load();
            }
        }
    }
}
