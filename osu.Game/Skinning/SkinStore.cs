// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public class SkinStore : DatabaseBackedStore, IAddableStore<SkinInfo>
    {
        public SkinStore(DatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        public void Add(SkinInfo item)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;
                context.SkinInfo.Attach(item);
            }
        }
    }
}
