// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Scoring
{
    public class ScoreManager : DownloadableArchiveModelManager<ScoreInfo, ScoreFileInfo>
    {
        public override string[] HandledExtensions => new[] { ".osr" };

        protected override string[] HashableFileTypes => new[] { ".osr" };

        protected override string ImportFromStablePath => Path.Combine("Data", "r");

        private readonly RulesetStore rulesets;
        private readonly Func<BeatmapManager> beatmaps;

        public ScoreManager(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, IAPIProvider api, IDatabaseContextFactory contextFactory, IIpcHost importHost = null)
            : base(storage, contextFactory, api, new ScoreStore(contextFactory, storage), importHost)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
        }

        protected override ScoreInfo CreateModel(ArchiveReader archive)
        {
            if (archive == null)
                return null;

            using (var stream = archive.GetStream(archive.Filenames.First(f => f.EndsWith(".osr"))))
            {
                try
                {
                    return new DatabasedLegacyScoreParser(rulesets, beatmaps()).Parse(stream).ScoreInfo;
                }
                catch (LegacyScoreParser.BeatmapNotFoundException e)
                {
                    Logger.Log(e.Message, LoggingTarget.Information, LogLevel.Error);
                    return null;
                }
            }
        }

        protected override IEnumerable<string> GetStableImportPaths(Storage stableStorage)
            => stableStorage.GetFiles(ImportFromStablePath).Where(p => HandledExtensions.Any(ext => Path.GetExtension(p)?.Equals(ext, StringComparison.InvariantCultureIgnoreCase) ?? false));

        public Score GetScore(ScoreInfo score) => new LegacyDatabasedScore(score, rulesets, beatmaps(), Files.Store);

        public List<ScoreInfo> GetAllUsableScores() => ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

        public IEnumerable<ScoreInfo> QueryScores(Expression<Func<ScoreInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().Where(query);

        public ScoreInfo Query(Expression<Func<ScoreInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        protected override ArchiveDownloadRequest<ScoreInfo> CreateDownloadRequest(ScoreInfo score, bool minimiseDownload) => new DownloadReplayRequest(score);

        protected override bool CheckLocalAvailability(ScoreInfo model, IQueryable<ScoreInfo> items) => items.Any(s => s.OnlineScoreID == model.OnlineScoreID && s.Files.Any());
    }
}
