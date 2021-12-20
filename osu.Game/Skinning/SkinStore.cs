// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class SkinStore : MutableDatabaseBackedStoreWithFileIncludes<EFSkinInfo, SkinFileInfo>
    {
        public SkinStore(DatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }
    }
}
