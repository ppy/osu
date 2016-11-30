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

namespace osu.Game.Overlays.Options.General
{
    public class LoginOptions : OptionsSubsection, IOnlineComponent
    {
        private Container loginForm;
        protected override string Header => "Sign In";

        public LoginOptions()
        {
            Children = new[]
            {
                loginForm = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[] { new LoadingAnimation() }
                }
            };
        }

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
                    loginForm.Children = new Drawable[]
                    {
                        new LoginForm(api)
                    };
                    break;
                case APIState.Failing:
                    loginForm.Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = @"Connection failing :(",
                        },
                    };
                    break;
                case APIState.Connecting:
                    loginForm.Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = @"Connecting...",
                        },
                    };
                    break;
                case APIState.Online:
                    loginForm.Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = $@"Connected as {api.Username}!",
                        },
                    };
                    break;
            }
        }

        class LoginForm : FlowContainer
        {
            private APIAccess api;

            private TextBox username;
            private TextBox password;

            public LoginForm(APIAccess api)
            {
                Direction = FlowDirection.VerticalOnly;
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                Spacing = new Vector2(0, 5);
                // TODO: Wire things up
                Children = new Drawable[]
                {
                    new SpriteText { Text = "Username" },
                    username = new TextBox { Height = 20, RelativeSizeAxes = Axes.X, Text = api?.Username ?? string.Empty },
                    new SpriteText { Text = "Password" },
                    password = new PasswordTextBox { Height = 20, RelativeSizeAxes = Axes.X },
                    new OsuButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Log in",
                        Action = performLogin
                    }
                };
            }

            private void performLogin()
            {
                api.Login(username.Text, password.Text);
            }

            [BackgroundDependencyLoader(permitNulls: true)]
            private void load(APIAccess api)
            {
                this.api = api;
            }
        }
    }
}
