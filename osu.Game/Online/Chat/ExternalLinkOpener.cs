// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Online.Chat
{
    public partial class ExternalLinkOpener : Component
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private Clipboard clipboard { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private IDialogOverlay? dialogOverlay { get; set; }

        private Bindable<bool> externalLinkWarning = null!;

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config)
        {
            externalLinkWarning = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning);
        }

        public void OpenUrlExternally(string url, bool bypassWarning = false)
        {
            if (!bypassWarning && externalLinkWarning.Value && dialogOverlay != null)
                dialogOverlay.Push(new ExternalLinkDialog(url, () => host.OpenUrlExternally(url), () => clipboard.SetText(url)));
            else
                host.OpenUrlExternally(url);
        }

        public partial class ExternalLinkDialog : PopupDialog
        {
            public ExternalLinkDialog(string url, Action openExternalLinkAction, Action copyExternalLinkAction)
            {
                HeaderText = "Just checking...";
                BodyText = $"You are about to leave osu! and open the following link in a web browser:\n\n{url}";

                Icon = FontAwesome.Solid.ExclamationTriangle;

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = @"Yes. Go for it.",
                        Action = openExternalLinkAction
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Copy URL to the clipboard instead.",
                        Action = copyExternalLinkAction
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"No! Abort mission!"
                    },
                };
            }
        }
    }
}
