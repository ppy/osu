// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.AccountCreation
{
    public partial class ScreenEntry : AccountCreationScreen
    {
        private ErrorTextFlowContainer usernameDescription = null!;
        private ErrorTextFlowContainer emailAddressDescription = null!;
        private ErrorTextFlowContainer passwordDescription = null!;

        private OsuTextBox usernameTextBox = null!;
        private OsuTextBox emailTextBox = null!;
        private OsuPasswordTextBox passwordTextBox = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IBindable<APIState> apiState = null!;

        private ShakeContainer registerShake = null!;
        private ITextPart characterCheckText = null!;

        private OsuTextBox[] textboxes = null!;
        private LoadingLayer loadingLayer = null!;

        [Resolved]
        private GameHost? host { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Margin = new MarginPadding { Vertical = 10 },
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.GetFont(size: 20),
                            Text = AccountCreationStrings.LetsCreateAnAccount
                        },
                        usernameTextBox = new OsuTextBox
                        {
                            PlaceholderText = UsersStrings.LoginUsername.ToLower(),
                            RelativeSizeAxes = Axes.X,
                            TabbableContentContainer = this
                        },
                        usernameDescription = new ErrorTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                        emailTextBox = new OsuTextBox
                        {
                            PlaceholderText = ModelValidationStrings.UserAttributesUserEmail.ToLower(),
                            RelativeSizeAxes = Axes.X,
                            TabbableContentContainer = this
                        },
                        emailAddressDescription = new ErrorTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                        passwordTextBox = new OsuPasswordTextBox
                        {
                            PlaceholderText = UsersStrings.LoginPassword.ToLower(),
                            RelativeSizeAxes = Axes.X,
                            TabbableContentContainer = this,
                        },
                        passwordDescription = new ErrorTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                registerShake = new ShakeContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Child = new SettingsButton
                                    {
                                        Text = LoginPanelStrings.Register,
                                        Margin = new MarginPadding { Vertical = 20 },
                                        Action = performRegistration
                                    }
                                }
                            }
                        },
                    },
                },
                loadingLayer = new LoadingLayer(true)
            };

            textboxes = new[] { usernameTextBox, emailTextBox, passwordTextBox };

            usernameDescription.AddText(AccountCreationStrings.UsernameDescription);

            emailAddressDescription.AddText(AccountCreationStrings.EmailDescription1);
            emailAddressDescription.AddText(AccountCreationStrings.EmailDescription2, cp => cp.Font = cp.Font.With(Typeface.Torus, weight: FontWeight.Bold));

            passwordDescription.AddText("At least ");
            characterCheckText = passwordDescription.AddText("8 characters long");
            passwordDescription.AddText(". Choose something long but also something you will remember, like a line from your favourite song.");

            passwordTextBox.Current.BindValueChanged(_ => updateCharacterCheckTextColour(), true);
            characterCheckText.DrawablePartsRecreated += _ => updateCharacterCheckTextColour();

            apiState = api.State.GetBoundCopy();
        }

        private void updateCharacterCheckTextColour()
        {
            string password = passwordTextBox.Text;

            foreach (var d in characterCheckText.Drawables)
                d.Colour = password.Length == 0 ? Color4.White : Interpolation.ValueAt(password.Length, Color4.OrangeRed, Color4.YellowGreen, 0, 8, Easing.In);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            loadingLayer.Hide();

            if (host?.OnScreenKeyboardOverlapsGameWindow != true)
                focusNextTextBox();
        }

        private void performRegistration()
        {
            if (focusNextTextBox())
            {
                registerShake.Shake();
                return;
            }

            usernameDescription.ClearErrors();
            emailAddressDescription.ClearErrors();
            passwordDescription.ClearErrors();

            loadingLayer.Show();

            Task.Run(() =>
            {
                bool success;
                RegistrationRequest.RegistrationRequestErrors? errors = null;

                try
                {
                    errors = api.CreateAccount(emailTextBox.Text, usernameTextBox.Text, passwordTextBox.Text);
                    success = errors == null;
                }
                catch (Exception)
                {
                    success = false;
                }

                Schedule(() =>
                {
                    if (!success)
                    {
                        if (errors != null)
                        {
                            if (errors.User != null)
                            {
                                usernameDescription.AddErrors(errors.User.Username);
                                emailAddressDescription.AddErrors(errors.User.Email);
                                passwordDescription.AddErrors(errors.User.Password);
                            }

                            if (!string.IsNullOrEmpty(errors.Redirect))
                            {
                                if (!string.IsNullOrEmpty(errors.Message))
                                    passwordDescription.AddErrors(new[] { errors.Message });

                                game?.OpenUrlExternally($"{errors.Redirect}?username={usernameTextBox.Text}&email={emailTextBox.Text}", true);
                            }
                        }
                        else
                        {
                            passwordDescription.AddErrors(new[] { "Something happened... but we're not sure what." });
                        }

                        registerShake.Shake();
                        loadingLayer.Hide();
                        return;
                    }

                    apiState.BindValueChanged(state =>
                    {
                        if (state.NewValue == APIState.RequiresSecondFactorAuth)
                            this.Push(new ScreenEmailVerification());
                    });

                    api.Login(usernameTextBox.Text, passwordTextBox.Text);
                });
            });
        }

        private bool focusNextTextBox()
        {
            var nextTextBox = nextUnfilledTextBox();

            if (nextTextBox != null)
            {
                Schedule(() => GetContainingInputManager().ChangeFocus(nextTextBox));
                return true;
            }

            return false;
        }

        private OsuTextBox? nextUnfilledTextBox() => textboxes.FirstOrDefault(t => string.IsNullOrEmpty(t.Text));
    }
}
