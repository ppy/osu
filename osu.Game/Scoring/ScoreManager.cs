// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Scoring
{
    public class ScoreManager : ArchiveModelManager<Score, ScoreFileInfo>
    {
        public override string[] HandledExtensions => new[] { ".osr" };

        protected override string ImportFromStablePath => "Replays";

        private readonly RulesetStore rulesets;
        private readonly BeatmapManager beatmaps;

        private readonly ScoreStore scores;

        public ScoreManager(RulesetStore rulesets, BeatmapManager beatmaps, Storage storage, IDatabaseContextFactory contextFactory, IIpcHost importHost = null)
            : base(storage, contextFactory, new ScoreStore(contextFactory, storage), importHost)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;

            scores = (ScoreStore)ModelStore;
        }

        protected override Score CreateModel(ArchiveReader archive)
        {
            if (archive == null)
                return null;

            using (var stream = archive.GetStream(archive.Filenames.First(f => f.EndsWith(".osr"))))
                return new DatabasedLegacyScoreParser(rulesets, beatmaps).Parse(stream);
        }

        protected override Score CheckForExisting(Score model)
        {
            var existingHashMatch = scores.ConsumableItems.FirstOrDefault(s => s.MD5Hash == model.MD5Hash);
            if (existingHashMatch != null)
            {
                Undelete(existingHashMatch);
                return existingHashMatch;
            }

            return null;
        }

        public List<Score> GetAllScores() => ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

        public Score Query(Expression<Func<Score, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().FirstOrDefault(query);
    }
}
