// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Online.Chat
{
    public partial class ExternalLinkOpener : Component
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private Clipboard clipboard { get; set; } = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Bindable<bool> externalLinkWarning = null!;

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config)
        {
            externalLinkWarning = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning);
        }

        public void OpenUrlExternally(string url, LinkWarnMode warnMode = LinkWarnMode.Default)
        {
            bool isTrustedDomain;

            if (url.StartsWith('/'))
            {
                url = $"{api.Endpoints.WebsiteUrl}{url}";
                isTrustedDomain = true;
            }
            else
            {
                isTrustedDomain = url.StartsWith(api.Endpoints.WebsiteUrl, StringComparison.Ordinal);
            }

            if (!url.CheckIsValidUrl())
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Text = NotificationsStrings.UnsupportedOrDangerousUrlProtocol(url),
                });

                return;
            }

            bool shouldWarn;

            switch (warnMode)
            {
                case LinkWarnMode.Default:
                    shouldWarn = externalLinkWarning.Value && !isTrustedDomain;
                    break;

                case LinkWarnMode.AlwaysWarn:
                    shouldWarn = true;
                    break;

                case LinkWarnMode.NeverWarn:
                    shouldWarn = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(warnMode), warnMode, null);
            }

            if (dialogOverlay != null && shouldWarn)
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
