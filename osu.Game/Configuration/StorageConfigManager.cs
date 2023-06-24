// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Configuration
{
    public class StorageConfigManager : IniConfigManager<StorageConfig>
    {
        protected override string Filename => "storage.ini";

        public StorageConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(StorageConfig.FullPath, string.Empty);
        }
    }

    public enum StorageConfig
    {
        FullPath,
    }
}
