// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Login
{
    public partial class SecondFactorAuthForm : Container
    {
        private ErrorTextFlowContainer errorText = null!;

        private LoadingLayer loading = null!;
        private FillFlowContainer contentFlow = null!;
        private OsuTextBox codeTextBox = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS };

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, SettingsSection.ITEM_SPACING),
                    Children = new Drawable[]
                    {
                        contentFlow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, SettingsSection.ITEM_SPACING),
                        },
                        errorText = new ErrorTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0,
                        },
                    }
                },
                loading = new LoadingLayer(true)
                {
                    Padding = new MarginPadding { Vertical = -SettingsSection.ITEM_SPACING },
                }
            };

            if (api.LastLoginError?.Message is string error)
            {
                errorText.Alpha = 1;
                errorText.AddErrors(new[] { error });
            }

            showContent(api.SessionVerificationMethod!.Value);
        }

        private void showContent(SessionVerificationMethod sessionVerificationMethod)
        {
            switch (sessionVerificationMethod)
            {
                case SessionVerificationMethod.EmailMessage:
                    showEmailVerification();
                    break;

                case SessionVerificationMethod.TimedOneTimePassword:
                    showTotpVerification();
                    break;
            }
        }

        private void showEmailVerification()
        {
            LinkFlowContainer explainText;

            contentFlow.Clear();
            contentFlow.AddRange(new Drawable[]
            {
                new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Text = "An email has been sent to you with a verification code. Enter the code.",
                },
                codeTextBox = new OsuTextBox
                {
                    InputProperties = new TextInputProperties(TextInputType.Code),
                    PlaceholderText = "Enter code",
                    RelativeSizeAxes = Axes.X,
                    TabbableContentContainer = this,
                },
                explainText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            });

            explainText.AddParagraph(UserVerificationStrings.BoxInfoCheckSpam);
            // We can't support localisable strings with nested links yet. Not sure if we even can (probably need to allow markdown link formatting or something).
            explainText.AddParagraph("If you can't access your email or have forgotten what you used, please follow the ");
            explainText.AddLink(UserVerificationStrings.BoxInfoRecoverLink, $"{api.Endpoints.WebsiteUrl}/home/password-reset");
            explainText.AddText(". You can also ");
            explainText.AddLink(UserVerificationStrings.BoxInfoReissueLink, () =>
            {
                loading.Show();

                var reissueRequest = new ReissueVerificationCodeRequest();
                reissueRequest.Failure += ex =>
                {
                    Logger.Error(ex, @"Failed to retrieve new verification code.");
                    loading.Hide();
                };
                reissueRequest.Success += () =>
                {
                    loading.Hide();
                };

                Task.Run(() => api.Perform(reissueRequest));
            });
            explainText.AddText(" or ");
            explainText.AddLink(UserVerificationStrings.BoxInfoLogoutLink, () => { api.Logout(); });
            explainText.AddText(".");

            codeTextBox.Current.BindValueChanged(code =>
            {
                string trimmedCode = code.NewValue.Trim();

                if (trimmedCode.Length == 8)
                {
                    api.AuthenticateSecondFactor(trimmedCode);
                    codeTextBox.Current.Disabled = true;
                }
            });
        }

        private void showTotpVerification()
        {
            LinkFlowContainer explainText;

            contentFlow.Clear();
            contentFlow.AddRange(new Drawable[]
            {
                new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Text = "Please enter the code from your authenticator app.",
                },
                codeTextBox = new OsuNumberBox
                {
                    InputProperties = new TextInputProperties(TextInputType.NumericalPassword),
                    PlaceholderText = "Enter code",
                    RelativeSizeAxes = Axes.X,
                    TabbableContentContainer = this,
                },
                explainText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            });

            // We can't support localisable strings with nested links yet. Not sure if we even can (probably need to allow markdown link formatting or something).
            explainText.AddParagraph("If you can't access your app, ");
            explainText.AddLink("you can verify using email instead", () =>
            {
                var fallbackRequest = new VerificationMailFallbackRequest();
                fallbackRequest.Success += showEmailVerification;
                fallbackRequest.Failure += ex => errorText.Text = ex.Message;
                Task.Run(() => api.Perform(fallbackRequest));
            });
            explainText.AddText(". You can also ");
            explainText.AddLink(UserVerificationStrings.BoxInfoLogoutLink, () => { api.Logout(); });
            explainText.AddText(".");

            codeTextBox.Current.BindValueChanged(code =>
            {
                string trimmedCode = code.NewValue.Trim();

                if (trimmedCode.Length == 6)
                {
                    api.AuthenticateSecondFactor(trimmedCode);
                    codeTextBox.Current.Disabled = true;
                }
            });
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override void OnFocus(FocusEvent e)
        {
            Schedule(() => { GetContainingFocusManager()!.ChangeFocus(codeTextBox); });
        }
    }
}
