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
        private OsuGame? game { get; set; }

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

        public void OpenUrlExternally(string userInput, LinkWarnMode warnMode = LinkWarnMode.Default)
        {
            bool isTrustedDomain = false;
            var websiteUri = new Uri(api.Endpoints.WebsiteUrl);

            // this intentionally handles BOTH root-relative (`/abc`) and absolute paths
            // using checks such as `userInput.StartsWith('/')` is NOT valid because of protocol-relative URIs (e.g. `//evil.test`)
            Uri.TryCreate(websiteUri, userInput, out var parsedUri);

            switch (parsedUri?.Scheme)
            {
                case @"http":
                case @"https":
                    if (!string.IsNullOrEmpty(parsedUri.UserInfo))
                        goto default;

                    // `Uri` normalises case of everything prior to URI path to lowercase
                    if (string.Equals(parsedUri.Host, websiteUri.Host, StringComparison.Ordinal))
                    {
                        if (websiteUri.Port != parsedUri.Port)
                            goto default;

                        isTrustedDomain = true;
                    }
                    else if (!parsedUri.IsDefaultPort)
                        goto default;

                    break;

                case @"mailto":
                    // allowed but not trusted
                    break;

                default:
                    notificationOverlay?.Post(new SimpleErrorNotification
                    {
                        Text = NotificationsStrings.UnsupportedOrDangerousUrlProtocol(userInput),
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

            string stringifiedUri = parsedUri.ToString();

            if (dialogOverlay != null && shouldWarn)
                dialogOverlay.Push(new ExternalLinkDialog(stringifiedUri, () => host.OpenUrlExternally(stringifiedUri), () => game?.CopyToClipboard(stringifiedUri)));
            else
                host.OpenUrlExternally(stringifiedUri);
        }

        public partial class ExternalLinkDialog : PopupDialog
        {
            public ExternalLinkDialog(string url, Action openExternalLinkAction, Action copyExternalLinkAction)
            {
                HeaderText = DialogStrings.CautionHeaderText;
                BodyText = DialogStrings.ExternalLinkBodyText(url);

                Icon = FontAwesome.Solid.ExclamationTriangle;

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = DialogStrings.ExternalLinkOkButton,
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
