// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class SkinDeleteDialog : PopupDialog
    {
        [Resolved]
        private SkinManager manager { get; set; }

        public SkinDeleteDialog(Skin skin)
        {
            BodyText = skin.SkinInfo.Value.Name;
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

                        manager.Delete(skin.SkinInfo.Value);
                        manager.CurrentSkinInfo.SetDefault();
                    },
                },
                new PopupDialogCancelButton
                {
                    Text = @"Firetruck, I didn't mean to!",
                },
            };
        }
    }
}
