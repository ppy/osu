// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
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
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

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

            updateLastError();

            showContent(api.SessionVerificationMethod!.Value);
            apiState.BindTo(api.State);
        }

        private void updateLastError()
        {
            if (api.LastLoginError?.Message is string error)
            {
                errorText.Alpha = 1;
                errorText.AddErrors(new[] { error });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiState.BindValueChanged(val =>
            {
                // this handles failed verifications.
                // in the case of failed verifications, `apiState` will briefly change to `Connecting` and then revert to `RequiresSecondFactorAuth`.
                // the login overlay doesn't need this logic as it will construct a new instance of this screen anyway,
                // but the *registration* overlay has no such logic and thus needs special handling.
                if (val.NewValue == APIState.RequiresSecondFactorAuth)
                {
                    // scheduling required as `APIAccess.State` value can be changed from threads that aren't update
                    // see: `APIAccess.run()` (which is given a dedicated thread) calls `APIAccess.attemptConnect()` which mutates `APIAccess.State`
                    Schedule(() =>
                    {
                        codeTextBox.Current.Disabled = false;
                        codeTextBox.Current.Value = string.Empty;
                        updateLastError();
                    });
                }
            });
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
                    Text = LoginPanelStrings.CodeSent,
                },
                codeTextBox = new OsuTextBox
                {
                    InputProperties = new TextInputProperties(TextInputType.Code),
                    PlaceholderText = LoginPanelStrings.EnterCode,
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
                    Text = UserVerificationStrings.BoxTotpHeading,
                },
                codeTextBox = new OsuNumberBox
                {
                    InputProperties = new TextInputProperties(TextInputType.NumericalPassword),
                    PlaceholderText = LoginPanelStrings.EnterCode,
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
