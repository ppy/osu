// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.IO
{
    public class OsuStorage : WrappedStorage
    {
        public OsuStorage(GameHost host)
            : base(host.Storage, string.Empty)
        {
            var storageConfig = new StorageConfigManager(host.Storage);

            var customStoragePath = storageConfig.Get<string>(StorageConfig.FullPath);

            if (!string.IsNullOrEmpty(customStoragePath))
            {
                ChangeTargetStorage(host.GetStorage(customStoragePath));
                Logger.Storage = UnderlyingStorage.GetStorageForDirectory("logs");
            }
        }
    }
}
