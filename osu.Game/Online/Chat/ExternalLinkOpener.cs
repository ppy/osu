// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

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
                HeaderText = DialogStrings.CautionHeaderText;
                BodyText = $"Are you sure you want to open the following link in a web browser?\n\n{url}";

                Icon = FontAwesome.Solid.ExclamationTriangle;

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = @"Open in browser",
                        Action = openExternalLinkAction
                    },
                    new PopupDialogCancelButton
                    {
                        Text = CommonStrings.CopyLink,
                        Action = copyExternalLinkAction
                    },
                    new PopupDialogCancelButton
                    {
                        Text = WebCommonStrings.ButtonsCancel,
                    },
                };
            }
        }
    }
}
