// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Skinning;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class SkinDeleteDialog : DeleteConfirmationDialog
    {
        private readonly Skin skin;

        public SkinDeleteDialog(Skin skin)
        {
            this.skin = skin;
            BodyText = skin.SkinInfo.Value.Name;
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager manager)
        {
            DeleteAction = () =>
            {
                manager.Delete(skin.SkinInfo.Value);
                manager.CurrentSkinInfo.SetDefault();
            };
        }
    }
}
