// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using OpenTK;
using osu.Game.Users;
using System.ComponentModel;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Input.States;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class LoginSettings : FillFlowContainer, IOnlineComponent
    {
        private bool bounding = true;
        private LoginForm form;
        private OsuColour colours;

        private UserPanel panel;
        private UserDropdown dropdown;

        /// <summary>
        /// Called to request a hide of a parent displaying this container.
        /// </summary>
        public Action RequestHide;

        public override RectangleF BoundingBox => bounding ? base.BoundingBox : RectangleF.Empty;

        public bool Bounding
        {
            get { return bounding; }
            set
            {
                bounding = value;
                Invalidate(Invalidation.MiscGeometry);
            }
        }

        public LoginSettings()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0f, 5f);
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, APIAccess api)
        {
            this.colours = colours;

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
                        new OsuSpriteText
                        {
                            Text = "ACCOUNT",
                            Margin = new MarginPadding { Bottom = 5 },
                            Font = @"Exo2.0-Black",
                        },
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
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Connecting...",
                            Margin = new MarginPadding { Top = 10, Bottom = 10 },
                        },
                    };
                    break;
                case APIState.Online:
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Left = 20, Right = 20 },
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 10f),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "Signed in",
                                            TextSize = 18,
                                            Font = @"Exo2.0-Bold",
                                            Margin = new MarginPadding { Top = 5, Bottom = 5 },
                                        },
                                    },
                                },
                                panel = new UserPanel(api.LocalUser.Value)
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Action = RequestHide
                                },
                                dropdown = new UserDropdown { RelativeSizeAxes = Axes.X },
                            },
                        },
                    };

                    panel.Status.BindTo(api.LocalUser.Value.Status);

                    dropdown.Current.ValueChanged += newValue =>
                    {
                        switch (newValue)
                        {
                            case UserAction.Online:
                                api.LocalUser.Value.Status.Value = new UserStatusOnline();
                                dropdown.StatusColour = colours.Green;
                                break;
                            case UserAction.DoNotDisturb:
                                api.LocalUser.Value.Status.Value = new UserStatusDoNotDisturb();
                                dropdown.StatusColour = colours.Red;
                                break;
                            case UserAction.AppearOffline:
                                api.LocalUser.Value.Status.Value = new UserStatusOffline();
                                dropdown.StatusColour = colours.Gray7;
                                break;
                            case UserAction.SignOut:
                                api.Logout();
                                break;
                        }
                    };
                    dropdown.Current.TriggerChange();

                    break;
            }

            if (form != null) GetContainingInputManager()?.ChangeFocus(form);
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(InputState state) => true;

        protected override void OnFocus(InputState state)
        {
            if (form != null) GetContainingInputManager().ChangeFocus(form);
            base.OnFocus(state);
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
                        PlaceholderText = "Email address",
                        RelativeSizeAxes = Axes.X,
                        Text = api?.ProvidedUsername ?? string.Empty,
                        TabbableContentContainer = this
                    },
                    password = new OsuPasswordTextBox
                    {
                        PlaceholderText = "Password",
                        RelativeSizeAxes = Axes.X,
                        TabbableContentContainer = this,
                        OnCommit = (sender, newText) => performLogin()
                    },
                    new SettingsCheckbox
                    {
                        LabelText = "Remember email address",
                        Bindable = config.GetBindable<bool>(OsuSetting.SaveUsername),
                    },
                    new SettingsCheckbox
                    {
                        LabelText = "Stay signed in",
                        Bindable = config.GetBindable<bool>(OsuSetting.SavePassword),
                    },
                    new SettingsButton
                    {
                        Text = "Sign in",
                        Action = performLogin
                    },
                    new SettingsButton
                    {
                        Text = "Register",
                        //Action = registerLink
                    }
                };
            }

            public override bool AcceptsFocus => true;

            protected override bool OnClick(InputState state) => true;

            protected override void OnFocus(InputState state)
            {
                Schedule(() => { GetContainingInputManager().ChangeFocus(string.IsNullOrEmpty(username.Text) ? username : password); });
            }
        }

        private class UserDropdown : OsuEnumDropdown<UserAction>
        {
            protected override DropdownHeader CreateHeader() => new UserDropdownHeader();

            protected override DropdownMenu CreateMenu() => new UserDropdownMenu();

            public Color4 StatusColour
            {
                set
                {
                    var h = Header as UserDropdownHeader;
                    if (h == null) return;
                    h.StatusColour = value;
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Gray5;
            }

            private class UserDropdownMenu : OsuDropdownMenu
            {
                public UserDropdownMenu()
                {
                    Masking = true;
                    CornerRadius = 5;

                    Margin = new MarginPadding { Bottom = 5 };

                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 4,
                    };

                    ItemsContainer.Padding = new MarginPadding();
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = colours.Gray3;
                }

                protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableUserDropdownMenuItem(item);

                private class DrawableUserDropdownMenuItem : DrawableOsuDropdownMenuItem
                {
                    public DrawableUserDropdownMenuItem(MenuItem item)
                        : base(item)
                    {
                        Foreground.Padding = new MarginPadding { Top = 5, Bottom = 5, Left = 10, Right = 5 };
                        CornerRadius = 5;
                    }

                    protected override Drawable CreateContent() => new Content
                    {
                        Label = { Margin = new MarginPadding { Left = UserDropdownHeader.LABEL_LEFT_MARGIN - 11 } }
                    };
                }
            }

            private class UserDropdownHeader : OsuDropdownHeader
            {
                public const float LABEL_LEFT_MARGIN = 20;

                private readonly SpriteIcon statusIcon;
                public Color4 StatusColour
                {
                    set
                    {
                        statusIcon.FadeColour(value, 500, Easing.OutQuint);
                    }
                }

                public UserDropdownHeader()
                {
                    Foreground.Padding = new MarginPadding { Left = 10, Right = 10 };
                    Margin = new MarginPadding { Bottom = 5 };
                    Masking = true;
                    CornerRadius = 5;
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 4,
                    };

                    Icon.Size = new Vector2(14);
                    Icon.Margin = new MarginPadding(0);

                    Foreground.Add(statusIcon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.fa_circle_o,
                        Size = new Vector2(14),
                    });

                    Text.Margin = new MarginPadding { Left = LABEL_LEFT_MARGIN };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = colours.Gray3;
                }
            }
        }

        private enum UserAction
        {
            Online,
            [Description(@"Do not disturb")]
            DoNotDisturb,
            [Description(@"Appear offline")]
            AppearOffline,
            [Description(@"Sign out")]
            SignOut,
        }
    }
}
