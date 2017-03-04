// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Skins
{
    public class SkinManager
    {
        public static SkinInfo DEFAULT_SKIN = new SkinInfo {
            Name = @"default",
        };

        private Bindable<SkinInfo> bindable;
        private List<Skin> skins;
        private Skin selected;

        public SkinManager()
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            bindable = config.GetBindable<SkinInfo>(OsuConfig.Skin);
            bindable.ValueChanged += ChangedSkin;
            foreach (Skin skin in skins)
            {
                skin.UpdateSkin();
                if (skin.info.Path == bindable.Value.Path)
                    selected = skin;
            }
        }

        private void ChangedSkin(object sender, EventArgs e) {s
            foreach (Skin skin in skins) {
                if(skin.info.Path == bindable.Value.Path)
                    selected = skin;
            }
            selected.UpdateSkin();
        }
    }
}
