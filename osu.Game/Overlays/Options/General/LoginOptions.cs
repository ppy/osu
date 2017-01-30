//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.General
{
    public class LoginOptions : OptionsSubsection, IOnlineComponent
    {
        protected override string Header => "Sign In";

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api)
        {
            api?.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                    Children = new Drawable[]
                    {
                        new LoginForm()
                    };
                    break;
                case APIState.Failing:
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = "Connection failing :(",
                        },
                    };
                    break;
                case APIState.Connecting:
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = "Connecting...",
                        },
                    };
                    break;
                case APIState.Online:
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = $"Connected as {api.Username}!",
                        },
                        new OsuButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Sign out",
                            Action = api.Logout
                        }
                    };
                    break;
            }
        }

        class LoginForm : FlowContainer
        {
            private TextBox username;
            private TextBox password;
            private APIAccess api;

            private CheckBoxOption saveUsername;
            private CheckBoxOption savePassword;

            private void performLogin()
            {
                if (!string.IsNullOrEmpty(username.Text) && !string.IsNullOrEmpty(password.Text))
                    api.Login(username.Text, password.Text);
            }

            [BackgroundDependencyLoader(permitNulls: true)]
            private void load(APIAccess api, OsuConfigManager config)
            {
                this.api = api;
                Direction = FlowDirection.VerticalOnly;
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                Spacing = new Vector2(0, 5);
                Children = new Drawable[]
                {
                    new SpriteText { Text = "Username" },
                    username = new OsuTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = api?.Username ?? string.Empty
                    },
                    new SpriteText { Text = "Password" },
                    password = new OsuPasswordTextBox
                    {
                        RelativeSizeAxes = Axes.X
                    },
                    saveUsername = new CheckBoxOption
                    {
                        LabelText = "Remember Username",
                        Bindable = config.GetBindable<bool>(OsuConfig.SaveUsername),
                    },
                    savePassword = new CheckBoxOption
                    {
                        LabelText = "Remember Password",
                        Bindable = config.GetBindable<bool>(OsuConfig.SavePassword),
                    },
                    new OsuButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Log in",
                        Action = performLogin
                    },
                    new OsuButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Register",
                        //Action = registerLink
                    }
                };
            }
        }
    }
}
