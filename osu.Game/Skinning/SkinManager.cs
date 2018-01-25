// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class SkinManager
    {
        public Bindable<Skin> CurrentSkin = new Bindable<Skin>(new DefaultSkin());

        private SkinStore store;

        public SkinManager(DatabaseContextFactory contextFactory, Storage storage)
        {
            store = new SkinStore(contextFactory, storage);
        }

        public void Import(string[] paths)
        {
            throw new NotImplementedException();
        }
    }
}
