// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
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

        [CanBeNull]
        private readonly Func<BeatmapDifficultyManager> difficulties;

        [CanBeNull]
        private readonly OsuConfigManager configManager;

        public ScoreManager(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, IAPIProvider api, IDatabaseContextFactory contextFactory, IIpcHost importHost = null,
                            Func<BeatmapDifficultyManager> difficulties = null, OsuConfigManager configManager = null)
            : base(storage, contextFactory, api, new ScoreStore(contextFactory, storage), importHost)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
            this.difficulties = difficulties;
            this.configManager = configManager;
        }

        protected override ScoreInfo CreateModel(ArchiveReader archive)
        {
            if (archive == null)
                return null;

            using (var stream = archive.GetStream(archive.Filenames.First(f => f.EndsWith(".osr"))))
            {
                try
                {
                    return new DatabasedLegacyScoreDecoder(rulesets, beatmaps()).Parse(stream).ScoreInfo;
                }
                catch (LegacyScoreDecoder.BeatmapNotFoundException e)
                {
                    Logger.Log(e.Message, LoggingTarget.Information, LogLevel.Error);
                    return null;
                }
            }
        }

        protected override IEnumerable<string> GetStableImportPaths(Storage stableStorage)
            => stableStorage.GetFiles(ImportFromStablePath).Where(p => HandledExtensions.Any(ext => Path.GetExtension(p)?.Equals(ext, StringComparison.OrdinalIgnoreCase) ?? false));

        public Score GetScore(ScoreInfo score) => new LegacyDatabasedScore(score, rulesets, beatmaps(), Files.Store);

        public List<ScoreInfo> GetAllUsableScores() => ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

        public IEnumerable<ScoreInfo> QueryScores(Expression<Func<ScoreInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().Where(query);

        public ScoreInfo Query(Expression<Func<ScoreInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        protected override ArchiveDownloadRequest<ScoreInfo> CreateDownloadRequest(ScoreInfo score, bool minimiseDownload) => new DownloadReplayRequest(score);

        protected override bool CheckLocalAvailability(ScoreInfo model, IQueryable<ScoreInfo> items)
            => base.CheckLocalAvailability(model, items)
               || (model.OnlineScoreID != null && items.Any(i => i.OnlineScoreID == model.OnlineScoreID));

        public Bindable<long> GetTotalScore(ScoreInfo score)
        {
            var bindable = new TotalScoreBindable(score, difficulties);
            configManager?.BindWith(OsuSetting.ScoreDisplayMode, bindable.ScoringMode);
            return bindable;
        }

        public Bindable<string> GetTotalScoreString(ScoreInfo score) => new TotalScoreStringBindable(GetTotalScore(score));

        private class TotalScoreBindable : Bindable<long>
        {
            public readonly Bindable<ScoringMode> ScoringMode = new Bindable<ScoringMode>();

            private readonly ScoreInfo score;
            private readonly Func<BeatmapDifficultyManager> difficulties;

            public TotalScoreBindable(ScoreInfo score, Func<BeatmapDifficultyManager> difficulties)
            {
                this.score = score;
                this.difficulties = difficulties;

                Value = 0;

                ScoringMode.BindValueChanged(onScoringModeChanged, true);
            }

            private IBindable<StarDifficulty> difficultyBindable;

            private void onScoringModeChanged(ValueChangedEvent<ScoringMode> mode)
            {
                int? beatmapMaxCombo = score.Beatmap.MaxCombo;

                if (beatmapMaxCombo == null)
                {
                    if (score.Beatmap.ID == 0 || difficulties == null)
                    {
                        // We don't have enough information (max combo) to compute the score, so let's use the provided score.
                        Value = score.TotalScore;
                        return;
                    }

                    // We can compute the max combo locally after the async beatmap difficulty computation.
                    difficultyBindable = difficulties().GetBindableDifficulty(score.Beatmap, score.Ruleset, score.Mods);
                    difficultyBindable.BindValueChanged(d => updateScore(d.NewValue.MaxCombo), true);
                }
                else
                    updateScore(beatmapMaxCombo.Value);
            }

            private void updateScore(int beatmapMaxCombo)
            {
                if (beatmapMaxCombo == 0)
                {
                    Value = 0;
                    return;
                }

                var ruleset = score.Ruleset.CreateInstance();
                var scoreProcessor = ruleset.CreateScoreProcessor();

                scoreProcessor.Mods.Value = score.Mods;

                double maxBaseScore = 300 * beatmapMaxCombo;
                double maxHighestCombo = beatmapMaxCombo;

                Value = (long)Math.Round(scoreProcessor.GetScore(ScoringMode.Value, maxBaseScore, maxHighestCombo, score.Accuracy, score.MaxCombo / maxHighestCombo, 0));
            }
        }

        private class TotalScoreStringBindable : Bindable<string>
        {
            // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable (need to hold a reference)
            private readonly IBindable<long> totalScore;

            public TotalScoreStringBindable(IBindable<long> totalScore)
            {
                this.totalScore = totalScore;
                this.totalScore.BindValueChanged(v => Value = v.NewValue.ToString("N0"), true);
            }
        }
    }
}
