// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Users;
using System.ComponentModel;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;

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
                                panel = new UserPanel(api.LocalUser.Value) { RelativeSizeAxes = Axes.X },
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
                    new SettingsCheckbox
                    {
                        LabelText = "Remember username",
                        Bindable = config.GetBindable<bool>(OsuSetting.SaveUsername),
                    },
                    new SettingsCheckbox
                    {
                        LabelText = "Stay logged in",
                        Bindable = config.GetBindable<bool>(OsuSetting.SavePassword),
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

        private class UserDropdown : OsuEnumDropdown<UserAction>
        {
            protected override DropdownHeader CreateHeader() => new UserDropdownHeader { AccentColour = AccentColour };
            protected override Menu CreateMenu() => new UserDropdownMenu();
            protected override DropdownMenuItem<UserAction> CreateMenuItem(string text, UserAction value) => new UserDropdownMenuItem(text, value) { AccentColour = AccentColour };

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

            private class UserDropdownHeader : OsuDropdownHeader
            {
                public const float LABEL_LEFT_MARGIN = 20;

                private readonly TextAwesome statusIcon;
                public Color4 StatusColour
                {
                    set
                    {
                        statusIcon.FadeColour(value, 500, EasingTypes.OutQuint);
                    }
                }

                public UserDropdownHeader()
                {
                    Foreground.Padding = new MarginPadding { Left = 10, Right = 10 };
                    Margin = new MarginPadding { Bottom = 5 };
                    Masking = true;
                    CornerRadius = 5;
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 4,
                    };

                    Icon.TextSize = 14;
                    Icon.Margin = new MarginPadding(0);

                    Foreground.Add(statusIcon = new TextAwesome
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.fa_circle_o,
                        TextSize = 14,
                    });

                    Text.Margin = new MarginPadding { Left = LABEL_LEFT_MARGIN };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = colours.Gray3;
                }
            }

            private class UserDropdownMenu : OsuMenu
            {
                public UserDropdownMenu()
                {
                    Margin = new MarginPadding { Bottom = 5 };
                    CornerRadius = 5;
                    ItemsContainer.Padding = new MarginPadding(0);
                    Masking = true;
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 4,
                    };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Background.Colour = colours.Gray3;
                }
            }

            private class UserDropdownMenuItem : OsuDropdownMenuItem
            {
                public UserDropdownMenuItem(string text, UserAction current) : base(text, current)
                {
                    Foreground.Padding = new MarginPadding { Top = 5, Bottom = 5, Left = UserDropdownHeader.LABEL_LEFT_MARGIN, Right = 5 };
                    Chevron.Margin = new MarginPadding { Left = 2, Right = 3 };
                    CornerRadius = 5;
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
