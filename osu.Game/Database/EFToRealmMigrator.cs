// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;
using Realms;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

#nullable enable

namespace osu.Game.Database
{
    internal class EFToRealmMigrator : CompositeDrawable
    {
        public Task<bool> MigrationCompleted => migrationCompleted.Task;

        private readonly TaskCompletionSource<bool> migrationCompleted = new TaskCompletionSource<bool>();

        [Resolved]
        private DatabaseContextFactory efContextFactory { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private NotificationOverlay notificationOverlay { get; set; } = null!;

        [Resolved]
        private OsuGame game { get; set; } = null!;

        [Resolved]
        private Storage storage { get; set; } = null!;

        private readonly OsuSpriteText currentOperationText;

        public EFToRealmMigrator()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Database migration in progress",
                            Font = OsuFont.Default.With(size: 40)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "This could take a few minutes depending on the speed of your disk(s).",
                            Font = OsuFont.Default.With(size: 30)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Please keep the window open until this completes!",
                            Font = OsuFont.Default.With(size: 30)
                        },
                        new LoadingSpinner(true)
                        {
                            State = { Value = Visibility.Visible }
                        },
                        currentOperationText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Default.With(size: 30)
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beginMigration();
        }

        private void beginMigration()
        {
            Task.Factory.StartNew(() =>
            {
                using (var ef = efContextFactory.Get())
                {
                    realm.Write(r =>
                    {
                        // Before beginning, ensure realm is in an empty state.
                        // Migrations which are half-completed could lead to issues if the user tries a second time.
                        // Note that we only do this for beatmaps and scores since the other migrations are yonks old.
                        r.RemoveAll<BeatmapSetInfo>();
                        r.RemoveAll<BeatmapInfo>();
                        r.RemoveAll<BeatmapMetadata>();
                        r.RemoveAll<ScoreInfo>();
                    });

                    ef.Migrate();

                    migrateSettings(ef);
                    migrateSkins(ef);
                    migrateBeatmaps(ef);
                    migrateScores(ef);
                }
            }, TaskCreationOptions.LongRunning).ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    log("Migration successful!");

                    if (DebugUtils.IsDebugBuild)
                        Logger.Log("Your development database has been fully migrated to realm. If you switch back to a pre-realm branch and need your previous database, rename the backup file back to \"client.db\".\n\nNote that doing this can potentially leave your file store in a bad state.", level: LogLevel.Important);
                }
                else
                {
                    log("Migration failed!");
                    Logger.Log(t.Exception.ToString(), LoggingTarget.Database);

                    notificationOverlay.Post(new SimpleErrorNotification
                    {
                        Text = "IMPORTANT: During data migration, some of your data could not be successfully migrated. The previous version has been backed up.\n\nFor further assistance, please open a discussion on github and attach your backup files (click to get started).",
                        Activated = () =>
                        {
                            game.OpenUrlExternally($@"https://github.com/ppy/osu/discussions/new?title=Realm%20migration%20issue ({t.Exception.Message})&body=Please%20drag%20the%20""attach_me.zip""%20file%20here!&category=q-a", true);

                            const string attachment_filename = "attach_me.zip";
                            const string backup_folder = "backups";

                            var backupStorage = storage.GetStorageForDirectory(backup_folder);

                            backupStorage.Delete(attachment_filename);

                            try
                            {
                                using (var zip = ZipArchive.Create())
                                {
                                    zip.AddAllFromDirectory(backupStorage.GetFullPath(string.Empty));
                                    zip.SaveTo(Path.Combine(backupStorage.GetFullPath(string.Empty), attachment_filename), new ZipWriterOptions(CompressionType.Deflate));
                                }
                            }
                            catch { }

                            backupStorage.PresentFileExternally(attachment_filename);

                            return true;
                        }
                    });
                }

                // Regardless of success, since the game is going to continue with startup let's move the ef database out of the way.
                // If we were to not do this, the migration would run another time the next time the user starts the game.
                deletePreRealmData();

                migrationCompleted.SetResult(true);
                efContextFactory.SetMigrationCompletion();
            });
        }

        private void deletePreRealmData()
        {
            // Delete the database permanently.
            // Will cause future startups to not attempt migration.
            efContextFactory.ResetDatabase();
        }

        private void log(string message)
        {
            Logger.Log(message, LoggingTarget.Database);
            Scheduler.AddOnce(m => currentOperationText.Text = m, message);
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

            log("Beginning beatmaps migration to realm");

            // previous entries in EF are removed post migration.
            if (!existingBeatmapSets.Any())
            {
                log("No beatmaps found to migrate");
                return;
            }

            int count = existingBeatmapSets.Count();

            realm.Run(r =>
            {
                log($"Found {count} beatmaps in EF");

                var transaction = r.BeginWrite();
                int written = 0;
                int missing = 0;

                try
                {
                    foreach (var beatmapSet in existingBeatmapSets)
                    {
                        if (++written % 1000 == 0)
                        {
                            transaction.Commit();
                            transaction = r.BeginWrite();
                            log($"Migrated {written}/{count} beatmaps...");
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

                        migrateFiles(beatmapSet, r, realmBeatmapSet);

                        foreach (var beatmap in beatmapSet.Beatmaps)
                        {
                            var ruleset = r.Find<RulesetInfo>(beatmap.RulesetInfo.ShortName);
                            var metadata = getBestMetadata(beatmap.Metadata, beatmapSet.Metadata);

                            if (ruleset == null)
                            {
                                log($"Skipping {++missing} beatmaps with missing ruleset");
                                continue;
                            }

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

                        r.Add(realmBeatmapSet);
                    }
                }
                finally
                {
                    transaction.Commit();
                }

                log($"Successfully migrated {count} beatmaps to realm");
            });
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

            log("Beginning scores migration to realm");

            // previous entries in EF are removed post migration.
            if (!existingScores.Any())
            {
                log("No scores found to migrate");
                return;
            }

            int count = existingScores.Count();

            realm.Run(r =>
            {
                log($"Found {count} scores in EF");

                var transaction = r.BeginWrite();
                int written = 0;
                int missing = 0;

                try
                {
                    foreach (var score in existingScores)
                    {
                        if (++written % 1000 == 0)
                        {
                            transaction.Commit();
                            transaction = r.BeginWrite();
                            log($"Migrated {written}/{count} scores...");
                        }

                        var beatmap = r.All<BeatmapInfo>().FirstOrDefault(b => b.Hash == score.BeatmapInfo.Hash);
                        var ruleset = r.Find<RulesetInfo>(score.Ruleset.ShortName);

                        if (beatmap == null || ruleset == null)
                        {
                            log($"Skipping {++missing} scores with missing ruleset or beatmap");
                            continue;
                        }

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

                        migrateFiles(score, r, realmScore);

                        r.Add(realmScore);
                    }
                }
                finally
                {
                    transaction.Commit();
                }

                log($"Successfully migrated {count} scores to realm");
            });
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

            realm.Run(r =>
            {
                using (var transaction = r.BeginWrite())
                {
                    // only migrate data if the realm database is empty.
                    // note that this cannot be written as: `r.All<SkinInfo>().All(s => s.Protected)`, because realm does not support `.All()`.
                    if (!r.All<SkinInfo>().Any(s => !s.Protected))
                    {
                        log($"Migrating {existingSkins.Count} skins");

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

                            migrateFiles(skin, r, realmSkin);

                            r.Add(realmSkin);

                            if (skin.ID == userSkinInt)
                                userSkinChoice.Value = realmSkin.ID.ToString();
                        }
                    }

                    transaction.Commit();
                }
            });
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

            log("Beginning settings migration to realm");

            realm.Run(r =>
            {
                using (var transaction = r.BeginWrite())
                {
                    // only migrate data if the realm database is empty.
                    if (!r.All<RealmRulesetSetting>().Any())
                    {
                        log($"Migrating {existingSettings.Count} settings");

                        foreach (var dkb in existingSettings)
                        {
                            if (dkb.RulesetID == null)
                                continue;

                            string? shortName = getRulesetShortNameFromLegacyID(dkb.RulesetID.Value);

                            if (string.IsNullOrEmpty(shortName))
                                continue;

                            r.Add(new RealmRulesetSetting
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
            });
        }

        private string? getRulesetShortNameFromLegacyID(long rulesetId) =>
            efContextFactory.Get().RulesetInfo.FirstOrDefault(r => r.ID == rulesetId)?.ShortName;
    }
}
