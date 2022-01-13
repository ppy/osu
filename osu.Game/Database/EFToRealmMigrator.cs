// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public EFToRealmMigrator(DatabaseContextFactory efContextFactory, RealmContextFactory realmContextFactory, OsuConfigManager config)
        {
            this.efContextFactory = efContextFactory;
            this.realmContextFactory = realmContextFactory;
            this.config = config;
        }

        public void Run()
        {
            using (var db = efContextFactory.GetForWrite())
            {
                migrateSettings(db);
                migrateSkins(db);

                migrateScores(db);
            }
        }

        private void migrateScores(DatabaseWriteUsage db)
        {
            // can be removed 20220730.
            var existingScores = db.Context.ScoreInfo
                                   .Include(s => s.Ruleset)
                                   .Include(s => s.BeatmapInfo)
                                   .Include(s => s.Files)
                                   .ThenInclude(f => f.FileInfo)
                                   .ToList();

            // previous entries in EF are removed post migration.
            if (!existingScores.Any())
                return;

            using (var realm = realmContextFactory.CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                // only migrate data if the realm database is empty.
                // note that this cannot be written as: `realm.All<ScoreInfo>().All(s => s.Protected)`, because realm does not support `.All()`.
                if (!realm.All<ScoreInfo>().Any())
                {
                    foreach (var score in existingScores)
                    {
                        var realmScore = new ScoreInfo
                        {
                            Hash = score.Hash,
                            DeletePending = score.DeletePending,
                            OnlineID = score.OnlineID ?? -1,
                            ModsJson = score.ModsJson,
                            StatisticsJson = score.StatisticsJson,
                            User = score.User,
                            TotalScore = score.TotalScore,
                            MaxCombo = score.MaxCombo,
                            Accuracy = score.Accuracy,
                            HasReplay = ((IScoreInfo)score).HasReplay,
                            Date = score.Date,
                            PP = score.PP,
                            BeatmapInfo = realm.All<BeatmapInfo>().First(b => b.Hash == score.BeatmapInfo.Hash),
                            Ruleset = realm.Find<RulesetInfo>(score.Ruleset.ShortName),
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

                db.Context.RemoveRange(existingScores);
                // Intentionally don't clean up the files, so they don't get purged by EF.

                transaction.Commit();
            }
        }

        private void migrateSkins(DatabaseWriteUsage db)
        {
            // can be removed 20220530.
            var existingSkins = db.Context.SkinInfo
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

                db.Context.RemoveRange(existingSkins);
                // Intentionally don't clean up the files, so they don't get purged by EF.

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

        private void migrateSettings(DatabaseWriteUsage db)
        {
            // migrate ruleset settings. can be removed 20220315.
            var existingSettings = db.Context.DatabasedSetting;

            // previous entries in EF are removed post migration.
            if (!existingSettings.Any())
                return;

            using (var realm = realmContextFactory.CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                // only migrate data if the realm database is empty.
                if (!realm.All<RealmRulesetSetting>().Any())
                {
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

                db.Context.RemoveRange(existingSettings);

                transaction.Commit();
            }
        }

        private string? getRulesetShortNameFromLegacyID(long rulesetId) =>
            efContextFactory.Get().RulesetInfo.FirstOrDefault(r => r.ID == rulesetId)?.ShortName;
    }
}
