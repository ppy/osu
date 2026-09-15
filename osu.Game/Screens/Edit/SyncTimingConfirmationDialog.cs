// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using osuTK;

namespace osu.Game.Screens.Edit
{
    public partial class SyncTimingConfirmationDialog : DangerousActionDialog
    {
        private readonly BindableBool syncBookmarks = new BindableBool(true);
        private readonly BindableBool syncPreviewPoint = new BindableBool(true);

        public SyncTimingConfirmationDialog(Action<bool, bool> syncAction)
        {
            HeaderText = EditorDialogsStrings.SyncTimingConfirmationHeader;
            BodyText = EditorDialogsStrings.SyncTimingConfirmationBody;

            DangerousAction = () => syncAction(syncBookmarks.Value, syncPreviewPoint.Value);

            MainContent.Child = new FillFlowContainer
            {
                Margin = new MarginPadding { Top = 20 },
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 400,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new OsuCheckbox
                    {
                        LabelText = EditorDialogsStrings.SyncTimingOptionBookmarks,
                        Current = { BindTarget = syncBookmarks },
                    },
                    new OsuCheckbox
                    {
                        LabelText = EditorDialogsStrings.SyncTimingOptionPreviewPoint,
                        Current = { BindTarget = syncPreviewPoint },
                    },
                }
            };
        }
    }
}
