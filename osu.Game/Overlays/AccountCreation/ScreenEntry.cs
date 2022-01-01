// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.AccountCreation
{
    public class ScreenEntry : AccountCreationScreen
    {
        private ErrorTextFlowContainer usernameDescription;
        private ErrorTextFlowContainer emailAddressDescription;
        private ErrorTextFlowContainer passwordDescription;

        private OsuTextBox usernameTextBox;
        private OsuTextBox emailTextBox;
        private OsuPasswordTextBox passwordTextBox;

        [Resolved]
        private IAPIProvider api { get; set; }

        private ShakeContainer registerShake;
        private ITextPart characterCheckText;

        private OsuTextBox[] textboxes;
        private LoadingLayer loadingLayer;

        [Resolved]
        private GameHost host { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
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
                            Text = "Let's create an account!",
                        },
                        usernameTextBox = new OsuTextBox
                        {
                            PlaceholderText = "username",
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
                            PlaceholderText = "email address",
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
                            PlaceholderText = "password",
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
                                        Text = "Register",
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

            usernameDescription.AddText("This will be your public presence. No profanity, no impersonation. Avoid exposing your own personal details, too!");

            emailAddressDescription.AddText("Will be used for notifications, account verification and in the case you forget your password. No spam, ever.");
            emailAddressDescription.AddText(" Make sure to get it right!", cp => cp.Font = cp.Font.With(Typeface.Torus, weight: FontWeight.Bold));

            passwordDescription.AddText("At least ");
            characterCheckText = passwordDescription.AddText("8 characters long");
            passwordDescription.AddText(". Choose something long but also something you will remember, like a line from your favourite song.");

            passwordTextBox.Current.BindValueChanged(_ => updateCharacterCheckTextColour(), true);
            characterCheckText.DrawablePartsRecreated += _ => updateCharacterCheckTextColour();
        }

        private void updateCharacterCheckTextColour()
        {
            string password = passwordTextBox.Text;

            foreach (var d in characterCheckText.Drawables)
                d.Colour = password.Length == 0 ? Color4.White : Interpolation.ValueAt(password.Length, Color4.OrangeRed, Color4.YellowGreen, 0, 8, Easing.In);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
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
                RegistrationRequest.RegistrationRequestErrors errors = null;

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
                            usernameDescription.AddErrors(errors.User.Username);
                            emailAddressDescription.AddErrors(errors.User.Email);
                            passwordDescription.AddErrors(errors.User.Password);
                        }
                        else
                        {
                            passwordDescription.AddErrors(new[] { "Something happened... but we're not sure what." });
                        }

                        registerShake.Shake();
                        loadingLayer.Hide();
                        return;
                    }

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

        private OsuTextBox nextUnfilledTextBox() => textboxes.FirstOrDefault(t => string.IsNullOrEmpty(t.Text));
    }
}
