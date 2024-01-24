// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Login
{
    public partial class SecondFactorAuthForm : FillFlowContainer
    {
        private OsuTextBox codeTextBox = null!;
        private LinkFlowContainer explainText = null!;
        private ErrorTextFlowContainer errorText = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, SettingsSection.ITEM_SPACING);

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS },
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, SettingsSection.ITEM_SPACING),
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = "An email has been sent to you with a verification code. Enter the code.",
                        },
                        codeTextBox = new OsuTextBox
                        {
                            PlaceholderText = "Enter code",
                            RelativeSizeAxes = Axes.X,
                            TabbableContentContainer = this,
                        },
                        explainText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        errorText = new ErrorTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0,
                        },
                    },
                },
                new LinkFlowContainer
                {
                    Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };

            explainText.AddParagraph(UserVerificationStrings.BoxInfoCheckSpam);
            // We can't support localisable strings with nested links yet. Not sure if we even can (probably need to allow markdown link formatting or something).
            explainText.AddParagraph("If you can't access your email or have forgotten what you used, please follow the ");
            explainText.AddLink(UserVerificationStrings.BoxInfoRecoverLink, $"{api.WebsiteRootUrl}/home/password-reset");
            explainText.AddText(". You can also ");
            explainText.AddLink(UserVerificationStrings.BoxInfoReissueLink, () =>
            {
                var reissueRequest = new ReissueVerificationCodeRequest();
                reissueRequest.Failure += ex => Logger.Error(ex, @"Failed to retrieve new verification code.");
                api.Perform(reissueRequest);
            });
            explainText.AddText(" or ");
            explainText.AddLink(UserVerificationStrings.BoxInfoLogoutLink, () => { api.Logout(); });
            explainText.AddText(".");

            codeTextBox.Current.BindValueChanged(code =>
            {
                if (code.NewValue.Length == 8)
                {
                    api.AuthenticateSecondFactor(code.NewValue);
                    codeTextBox.Current.Disabled = true;
                }
            });

            if (api.LastLoginError?.Message is string error)
            {
                errorText.Alpha = 1;
                errorText.AddErrors(new[] { error });
            }
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override void OnFocus(FocusEvent e)
        {
            Schedule(() => { GetContainingInputManager().ChangeFocus(codeTextBox); });
        }
    }
}
