// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osu.Game.Overlays.Dialog;
using osu.Game.Database;

namespace osu.Game.Screens.Select
{
    public class SkinDeleteDialog : PopupDialog
    {
        private SkinManager manager;

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager)
        {
            manager = skinManager;
        }

        public SkinDeleteDialog(Skin skin)
        {
            skin.SkinInfo.PerformRead(s =>
            {
                BodyText = s.Name;

                Icon = FontAwesome.Regular.TrashAlt;
                HeaderText = @"Confirm deletion of";
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogDangerousButton
                    {
                        Text = @"Yes. Totally. Delete it.",
                        Action = () =>
                        {
                            if (manager == null)
                                return;

                            manager.Delete(s);
                            manager.CurrentSkinInfo.Value = DefaultSkin.CreateInfo().ToLiveUnmanaged();
                        },
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Firetruck, I didn't mean to!",
                    },
                };
            });
        }
    }
}
