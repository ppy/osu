// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Skinning;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    internal class EFToRealmMigrator
    {
        private readonly DatabaseContextFactory efContextFactory;
        private readonly RealmContextFactory realmContextFactory;
        private readonly OsuConfigManager config;
        private readonly Storage storage;

        public EFToRealmMigrator(DatabaseContextFactory efContextFactory, RealmContextFactory realmContextFactory, OsuConfigManager config, Storage storage)
        {
            this.efContextFactory = efContextFactory;
            this.realmContextFactory = realmContextFactory;
            this.config = config;
            this.storage = storage;
        }

        public void Run()
        {
            createBackup();

            using (var ef = efContextFactory.Get())
            {
                migrateSettings(ef);
                migrateSkins(ef);
                migrateBeatmaps(ef);
                migrateScores(ef);
            }

            // Delete the database permanently.
            // Will cause future startups to not attempt migration.
            Logger.Log("Migration successful, deleting EF database", LoggingTarget.Database);
            efContextFactory.ResetDatabase();
        }

        private void migrateBeatmaps(OsuDbContext ef)
        {
            // can be removed 20220730.
            var existingBeatmapSets = ef.EFBeatmapSetInfo
                                        .Include(s => s.Beatmaps).ThenInclude(b => b.RulesetInfo)
                                        .Include(s => s.Beatmaps).ThenInclude(b => b.Metadata)
                                        .Include(s => s.Beatmaps).ThenInclude(b => b.BaseDifficulty)
                                        .Include(s => s.Files).ThenInclude(f => f.FileInfo)
                                        .Include(s => s.Metadata);

            Logger.Log("Beginning beatmaps migration to realm", LoggingTarget.Database);

            // previous entries in EF are removed post migration.
            if (!existingBeatmapSets.Any())
            {
                Logger.Log("No beatmaps found to migrate", LoggingTarget.Database);
                return;
            }

            int count = existingBeatmapSets.Count();

            using (var realm = realmContextFactory.CreateContext())
            {
                Logger.Log($"Found {count} beatmaps in EF", LoggingTarget.Database);

                // only migrate data if the realm database is empty.
                // note that this cannot be written as: `realm.All<BeatmapSetInfo>().All(s => s.Protected)`, because realm does not support `.All()`.
                if (realm.All<BeatmapSetInfo>().Any(s => !s.Protected))
                {
                    Logger.Log("Skipping migration as realm already has beatmaps loaded", LoggingTarget.Database);
                }
                else
                {
                    var transaction = realm.BeginWrite();
                    int written = 0;

                    try
                    {
                        foreach (var beatmapSet in existingBeatmapSets)
                        {
                            if (++written % 1000 == 0)
                            {
                                transaction.Commit();
                                transaction = realm.BeginWrite();
                                Logger.Log($"Migrated {written}/{count} beatmaps...", LoggingTarget.Database);
                            }

                            var realmBeatmapSet = new BeatmapSetInfo
                            {
                                OnlineID = beatmapSet.OnlineID ?? -1,
                                DateAdded = beatmapSet.DateAdded,
                                Status = beatmapSet.Status,
                                DeletePending = beatmapSet.DeletePending,
                                Hash = beatmapSet.Hash,
                                Protected = beatmapSet.Protected,
                            };

                            migrateFiles(beatmapSet, realm, realmBeatmapSet);

                            foreach (var beatmap in beatmapSet.Beatmaps)
                            {
                                var ruleset = realm.Find<RulesetInfo>(beatmap.RulesetInfo.ShortName);
                                var metadata = getBestMetadata(beatmap.Metadata, beatmapSet.Metadata);

                                var realmBeatmap = new BeatmapInfo(ruleset, new BeatmapDifficulty(beatmap.BaseDifficulty), metadata)
                                {
                                    DifficultyName = beatmap.DifficultyName,
                                    Status = beatmap.Status,
                                    OnlineID = beatmap.OnlineID ?? -1,
                                    Length = beatmap.Length,
                                    BPM = beatmap.BPM,
                                    Hash = beatmap.Hash,
                                    StarRating = beatmap.StarRating,
                                    MD5Hash = beatmap.MD5Hash,
                                    Hidden = beatmap.Hidden,
                                    AudioLeadIn = beatmap.AudioLeadIn,
                                    StackLeniency = beatmap.StackLeniency,
                                    SpecialStyle = beatmap.SpecialStyle,
                                    LetterboxInBreaks = beatmap.LetterboxInBreaks,
                                    WidescreenStoryboard = beatmap.WidescreenStoryboard,
                                    EpilepsyWarning = beatmap.EpilepsyWarning,
                                    SamplesMatchPlaybackRate = beatmap.SamplesMatchPlaybackRate,
                                    DistanceSpacing = beatmap.DistanceSpacing,
                                    BeatDivisor = beatmap.BeatDivisor,
                                    GridSize = beatmap.GridSize,
                                    TimelineZoom = beatmap.TimelineZoom,
                                    Countdown = beatmap.Countdown,
                                    CountdownOffset = beatmap.CountdownOffset,
                                    MaxCombo = beatmap.MaxCombo,
                                    Bookmarks = beatmap.Bookmarks,
                                    BeatmapSet = realmBeatmapSet,
                                };

                                realmBeatmapSet.Beatmaps.Add(realmBeatmap);
                            }

                            realm.Add(realmBeatmapSet);
                        }
                    }
                    finally
                    {
                        transaction.Commit();
                    }

                    Logger.Log($"Successfully migrated {count} beatmaps to realm", LoggingTarget.Database);
                }
            }
        }

        private BeatmapMetadata getBestMetadata(EFBeatmapMetadata? beatmapMetadata, EFBeatmapMetadata? beatmapSetMetadata)
        {
            var metadata = beatmapMetadata ?? beatmapSetMetadata ?? new EFBeatmapMetadata();

            return new BeatmapMetadata
            {
                Title = metadata.Title,
                TitleUnicode = metadata.TitleUnicode,
                Artist = metadata.Artist,
                ArtistUnicode = metadata.ArtistUnicode,
                Author =
                {
                    OnlineID = metadata.Author.Id,
                    Username = metadata.Author.Username,
                },
                Source = metadata.Source,
                Tags = metadata.Tags,
                PreviewTime = metadata.PreviewTime,
                AudioFile = metadata.AudioFile,
                BackgroundFile = metadata.BackgroundFile,
            };
        }

        private void migrateScores(OsuDbContext db)
        {
            // can be removed 20220730.
            var existingScores = db.ScoreInfo
                                   .Include(s => s.Ruleset)
                                   .Include(s => s.BeatmapInfo)
                                   .Include(s => s.Files)
                                   .ThenInclude(f => f.FileInfo);

            Logger.Log("Beginning scores migration to realm", LoggingTarget.Database);

            // previous entries in EF are removed post migration.
            if (!existingScores.Any())
            {
                Logger.Log("No scores found to migrate", LoggingTarget.Database);
                return;
            }

            int count = existingScores.Count();

            using (var realm = realmContextFactory.CreateContext())
            {
                Logger.Log($"Found {count} scores in EF", LoggingTarget.Database);

                // only migrate data if the realm database is empty.
                if (realm.All<ScoreInfo>().Any())
                {
                    Logger.Log("Skipping migration as realm already has scores loaded", LoggingTarget.Database);
                }
                else
                {
                    var transaction = realm.BeginWrite();
                    int written = 0;

                    try
                    {
                        foreach (var score in existingScores)
                        {
                            if (++written % 1000 == 0)
                            {
                                transaction.Commit();
                                transaction = realm.BeginWrite();
                                Logger.Log($"Migrated {written}/{count} scores...", LoggingTarget.Database);
                            }

                            var beatmap = realm.All<BeatmapInfo>().First(b => b.Hash == score.BeatmapInfo.Hash);
                            var ruleset = realm.Find<RulesetInfo>(score.Ruleset.ShortName);
                            var user = new RealmUser
                            {
                                OnlineID = score.User.OnlineID,
                                Username = score.User.Username
                            };

                            var realmScore = new ScoreInfo(beatmap, ruleset, user)
                            {
                                Hash = score.Hash,
                                DeletePending = score.DeletePending,
                                OnlineID = score.OnlineID ?? -1,
                                ModsJson = score.ModsJson,
                                StatisticsJson = score.StatisticsJson,
                                TotalScore = score.TotalScore,
                                MaxCombo = score.MaxCombo,
                                Accuracy = score.Accuracy,
                                HasReplay = ((IScoreInfo)score).HasReplay,
                                Date = score.Date,
                                PP = score.PP,
                                Rank = score.Rank,
                                HitEvents = score.HitEvents,
                                Passed = score.Passed,
                                Combo = score.Combo,
                                Position = score.Position,
                                Statistics = score.Statistics,
                                Mods = score.Mods,
                                APIMods = score.APIMods,
                            };

                            migrateFiles(score, realm, realmScore);

                            realm.Add(realmScore);
                        }
                    }
                    finally
                    {
                        transaction.Commit();
                    }

                    Logger.Log($"Successfully migrated {count} scores to realm", LoggingTarget.Database);
                }
            }
        }

        private void migrateSkins(OsuDbContext db)
        {
            // can be removed 20220530.
            var existingSkins = db.SkinInfo
                                  .Include(s => s.Files)
                                  .ThenInclude(f => f.FileInfo)
                                  .ToList();

            // previous entries in EF are removed post migration.
            if (!existingSkins.Any())
                return;

            var userSkinChoice = config.GetBindable<string>(OsuSetting.Skin);
            int.TryParse(userSkinChoice.Value, out int userSkinInt);

            switch (userSkinInt)
            {
                case EFSkinInfo.DEFAULT_SKIN:
                    userSkinChoice.Value = SkinInfo.DEFAULT_SKIN.ToString();
                    break;

                case EFSkinInfo.CLASSIC_SKIN:
                    userSkinChoice.Value = SkinInfo.CLASSIC_SKIN.ToString();
                    break;
            }

            using (var realm = realmContextFactory.CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                // only migrate data if the realm database is empty.
                // note that this cannot be written as: `realm.All<SkinInfo>().All(s => s.Protected)`, because realm does not support `.All()`.
                if (!realm.All<SkinInfo>().Any(s => !s.Protected))
                {
                    Logger.Log($"Migrating {existingSkins.Count} skins", LoggingTarget.Database);

                    foreach (var skin in existingSkins)
                    {
                        var realmSkin = new SkinInfo
                        {
                            Name = skin.Name,
                            Creator = skin.Creator,
                            Hash = skin.Hash,
                            Protected = false,
                            InstantiationInfo = skin.InstantiationInfo,
                        };

                        migrateFiles(skin, realm, realmSkin);

                        realm.Add(realmSkin);

                        if (skin.ID == userSkinInt)
                            userSkinChoice.Value = realmSkin.ID.ToString();
                    }
                }

                transaction.Commit();
            }
        }

        private static void migrateFiles<T>(IHasFiles<T> fileSource, Realm realm, IHasRealmFiles realmObject) where T : INamedFileInfo
        {
            foreach (var file in fileSource.Files)
            {
                var realmFile = realm.Find<RealmFile>(file.FileInfo.Hash);

                if (realmFile == null)
                    realm.Add(realmFile = new RealmFile { Hash = file.FileInfo.Hash });

                realmObject.Files.Add(new RealmNamedFileUsage(realmFile, file.Filename));
            }
        }

        private void migrateSettings(OsuDbContext db)
        {
            // migrate ruleset settings. can be removed 20220315.
            var existingSettings = db.DatabasedSetting.ToList();

            // previous entries in EF are removed post migration.
            if (!existingSettings.Any())
                return;

            Logger.Log("Beginning settings migration to realm", LoggingTarget.Database);

            using (var realm = realmContextFactory.CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                // only migrate data if the realm database is empty.
                if (!realm.All<RealmRulesetSetting>().Any())
                {
                    Logger.Log($"Migrating {existingSettings.Count} settings", LoggingTarget.Database);

                    foreach (var dkb in existingSettings)
                    {
                        if (dkb.RulesetID == null)
                            continue;

                        string? shortName = getRulesetShortNameFromLegacyID(dkb.RulesetID.Value);

                        if (string.IsNullOrEmpty(shortName))
                            continue;

                        realm.Add(new RealmRulesetSetting
                        {
                            Key = dkb.Key,
                            Value = dkb.StringValue,
                            RulesetName = shortName,
                            Variant = dkb.Variant ?? 0,
                        });
                    }
                }

                transaction.Commit();
            }
        }

        private string? getRulesetShortNameFromLegacyID(long rulesetId) =>
            efContextFactory.Get().RulesetInfo.FirstOrDefault(r => r.ID == rulesetId)?.ShortName;

        private void createBackup()
        {
            string migration = $"before_final_migration_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            efContextFactory.CreateBackup($"client.{migration}.db");
            realmContextFactory.CreateBackup($"client.{migration}.realm");

            using (var source = storage.GetStream("collection.db"))
            using (var destination = storage.GetStream($"collection.{migration}.db", FileAccess.Write, FileMode.CreateNew))
                source.CopyTo(destination);
        }
    }
}
