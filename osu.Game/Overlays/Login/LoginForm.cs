// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Login
{
    public class LoginForm : FillFlowContainer
    {
        private TextBox username;
        private TextBox password;
        private ShakeContainer shakeSignIn;

        [Resolved(CanBeNull = true)]
        private IAPIProvider api { get; set; }

        public Action RequestHide;

        private void performLogin()
        {
            if (!string.IsNullOrEmpty(username.Text) && !string.IsNullOrEmpty(password.Text))
                api?.Login(username.Text, password.Text);
            else
                shakeSignIn.Shake();
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuConfigManager config, AccountCreationOverlay accountCreation)
        {
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, 5);
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            ErrorTextFlowContainer errorText;

            Children = new Drawable[]
            {
                username = new OsuTextBox
                {
                    PlaceholderText = UsersStrings.LoginUsername.ToLower(),
                    RelativeSizeAxes = Axes.X,
                    Text = api?.ProvidedUsername ?? string.Empty,
                    TabbableContentContainer = this
                },
                password = new OsuPasswordTextBox
                {
                    PlaceholderText = UsersStrings.LoginPassword.ToLower(),
                    RelativeSizeAxes = Axes.X,
                    TabbableContentContainer = this,
                },
                errorText = new ErrorTextFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
                new SettingsCheckbox
                {
                    LabelText = "Remember username",
                    Current = config.GetBindable<bool>(OsuSetting.SaveUsername),
                },
                new SettingsCheckbox
                {
                    LabelText = "Stay signed in",
                    Current = config.GetBindable<bool>(OsuSetting.SavePassword),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        shakeSignIn = new ShakeContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new SettingsButton
                            {
                                Text = UsersStrings.LoginButton,
                                Action = performLogin
                            },
                        }
                    }
                },
                new SettingsButton
                {
                    Text = "Register",
                    Action = () =>
                    {
                        RequestHide();
                        accountCreation.Show();
                    }
                }
            };

            password.OnCommit += (sender, newText) => performLogin();

            if (api?.LastLoginError?.Message is string error)
                errorText.AddErrors(new[] { error });
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override void OnFocus(FocusEvent e)
        {
            Schedule(() => { GetContainingInputManager().ChangeFocus(string.IsNullOrEmpty(username.Text) ? username : password); });
        }
    }
}
