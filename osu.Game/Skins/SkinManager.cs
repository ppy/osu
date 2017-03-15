// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Skins
{
    public class SkinManager
    {
        public static SkinInfo DefaultSkin = new SkinInfo
        {
            Name = @"default",
            Path = null,
        };

        public Action PostChangeSkin;

        private Bindable<SkinInfo> bindable;

        private List<SkinInfo> skins;
        private List<Skin> skinContents;

        private SkinInfo selected;
        private Skin selectedContents;

        private Storage storage;

        private SkinDatabase database;

        public SkinManager(SkinDatabase database, Storage storage, OsuConfigManager config)
        {
            skins = new List<SkinInfo>();
            skinContents = new List<Skin>();

            this.storage = storage;
            this.database = database;

            bindable = config.GetBindable<SkinInfo>(OsuConfig.Skin);
            bindable.ValueChanged += changeSkinWrapper;
            changeSkin();
        }

        private void updateSkins()
        {
            skins.Clear();
            skins.Add(DefaultSkin);
            skins.AddRange(database.GetSkins());

            foreach (SkinInfo info in skins) 
            {
                skinContents.Add(new Skin(info));
            } 
        }

        public List<KeyValuePair<string, SkinInfo>> UpdateItems() {
            updateSkins();

            var list = new List<KeyValuePair<string, SkinInfo>>();
            foreach (SkinInfo skin in skins)
            {
                list.Add(new KeyValuePair<string, SkinInfo>(skin.Name, skin));
            }
            return list;
        }

        private Skin getSkinContents(SkinInfo skin)
        {
            return skinContents.Find((x) => (x.Info.Path == skin.Path));
        }

        private void changeSkinWrapper(object sender, EventArgs e) => changeSkin();

        private void changeSkin() 
        {
            updateSkins();
            foreach (SkinInfo skin in skins)
            {
                if (skin.Path == bindable.Value.Path)
                    selected = skin;
            }
            selectedContents = getSkinContents(selected);
            selectedContents.UpdateSkin(storage);
        }
    }
}
