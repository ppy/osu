// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Database;
using osu.Game.Skinning;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public partial class SkinDeleteDialog : DangerousActionDialog
    {
        private readonly Live<SkinInfo> skin;

        public SkinDeleteDialog(Live<SkinInfo> skin)
        {
            this.skin = skin;
            BodyText = skin.Value.Name;
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager manager)
        {
            DangerousAction = () =>
            {
                manager.Delete(skin.Value);
                manager.CurrentSkinInfo.SetDefault();
            };
        }
    }
}
