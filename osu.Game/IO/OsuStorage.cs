// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.IO
{
    public class OsuStorage : MigratableStorage
    {
        private readonly StorageConfigManager storageConfig;

        public override string[] IgnoreDirectories => new[] { "cache" };

        public override string[] IgnoreFiles => new[]
        {
            "framework.ini",
            "storage.ini"
        };

        public OsuStorage(GameHost host)
            : base(host.Storage, string.Empty)
        {
            storageConfig = new StorageConfigManager(host.Storage);

            var customStoragePath = storageConfig.Get<string>(StorageConfig.FullPath);

            if (!string.IsNullOrEmpty(customStoragePath))
                ChangeTargetStorage(host.GetStorage(customStoragePath));
        }

        protected override void ChangeTargetStorage(Storage newStorage)
        {
            base.ChangeTargetStorage(newStorage);
            Logger.Storage = UnderlyingStorage.GetStorageForDirectory("logs");
        }

        public override void Migrate(Storage newStorage)
        {
            base.Migrate(newStorage);
            storageConfig.Set(StorageConfig.FullPath, newStorage.GetFullPath("."));
            storageConfig.Save();
        }
    }
}
