﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using OpenTK;
using osu.Framework.Input;

namespace osu.Game.Overlays.Options.Sections.General
{
    public class LoginOptions : OptionsSubsection, IOnlineComponent
    {
        private bool bounding = true;
        private LoginForm form;

        protected override string Header => "Account";

        public override RectangleF BoundingBox => bounding ? base.BoundingBox : RectangleF.Empty;

        public bool Bounding
        {
            get { return bounding; }
            set
            {
                bounding = value;
                Invalidate(Invalidation.Geometry);
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api)
        {
            api?.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            form = null;

            switch (state)
            {
                case APIState.Offline:
                    Children = new Drawable[]
                    {
                        form = new LoginForm()
                    };
                    break;
                case APIState.Failing:
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Connection failing :(",
                        },
                    };
                    break;
                case APIState.Connecting:
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Connecting...",
                        },
                    };
                    break;
                case APIState.Online:
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
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

            form?.TriggerFocus();
        }

        protected override bool OnFocus(InputState state)
        {
            form?.TriggerFocus();
            return base.OnFocus(state);
        }

        private class LoginForm : FillFlowContainer
        {
            private TextBox username;
            private TextBox password;
            private APIAccess api;

            private void performLogin()
            {
                if (!string.IsNullOrEmpty(username.Text) && !string.IsNullOrEmpty(password.Text))
                    api.Login(username.Text, password.Text);
            }

            [BackgroundDependencyLoader(permitNulls: true)]
            private void load(APIAccess api, OsuConfigManager config)
            {
                this.api = api;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(0, 5);
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                Children = new Drawable[]
                {
                    username = new OsuTextBox
                    {
                        PlaceholderText = "Username",
                        RelativeSizeAxes = Axes.X,
                        Text = api?.Username ?? string.Empty,
                        TabbableContentContainer = this
                    },
                    password = new OsuPasswordTextBox
                    {
                        PlaceholderText = "Password",
                        RelativeSizeAxes = Axes.X,
                        TabbableContentContainer = this,
                        OnCommit = (sender, newText) => performLogin()
                    },
                    new OptionCheckbox
                    {
                        LabelText = "Remember username",
                        Bindable = config.GetBindable<bool>(OsuConfig.SaveUsername),
                    },
                    new OptionCheckbox
                    {
                        LabelText = "Stay logged in",
                        Bindable = config.GetBindable<bool>(OsuConfig.SavePassword),
                    },
                    new OsuButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Sign in",
                        Action = performLogin
                    },
                    new OsuButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Register new account",
                        //Action = registerLink
                    }
                };
            }

            protected override bool OnFocus(InputState state)
            {
                Schedule(() =>
                {
                    if (string.IsNullOrEmpty(username.Text))
                        username.TriggerFocus();
                    else
                        password.TriggerFocus();
                });

                return base.OnFocus(state);
            }
        }
    }
}
