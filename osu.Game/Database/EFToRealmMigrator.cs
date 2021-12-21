// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Game.Configuration;
using osu.Game.Models;
using osu.Game.Skinning;

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

                        foreach (var file in skin.Files)
                        {
                            var realmFile = realm.Find<RealmFile>(file.FileInfo.Hash);

                            if (realmFile == null)
                                realm.Add(realmFile = new RealmFile { Hash = file.FileInfo.Hash });

                            realmSkin.Files.Add(new RealmNamedFileUsage(realmFile, file.Filename));
                        }

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
