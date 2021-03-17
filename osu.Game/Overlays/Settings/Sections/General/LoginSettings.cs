// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osuTK;
using osu.Game.Users;
using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class LoginSettings : FillFlowContainer
    {
        private bool bounding = true;
        private LoginForm form;

        [Resolved]
        private OsuColour colours { get; set; }

        private UserGridPanel panel;
        private UserDropdown dropdown;

        /// <summary>
        /// Called to request a hide of a parent displaying this container.
        /// </summary>
        public Action RequestHide;

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private IAPIProvider api { get; set; }

        public override RectangleF BoundingBox => bounding ? base.BoundingBox : RectangleF.Empty;

        public bool Bounding
        {
            get => bounding;
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

        [BackgroundDependencyLoader]
        private void load()
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            form = null;

            switch (state.NewValue)
            {
                case APIState.Offline:
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "ACCOUNT",
                            Margin = new MarginPadding { Bottom = 5 },
                            Font = OsuFont.GetFont(weight: FontWeight.Bold),
                        },
                        form = new LoginForm
                        {
                            RequestHide = RequestHide
                        }
                    };
                    break;

                case APIState.Failing:
                case APIState.Connecting:
                    LinkFlowContainer linkFlow;

                    Children = new Drawable[]
                    {
                        new LoadingSpinner
                        {
                            State = { Value = Visibility.Visible },
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        linkFlow = new LinkFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            Text = state.NewValue == APIState.Failing ? "Connection is failing, will attempt to reconnect... " : "Attempting to connect... ",
                            Margin = new MarginPadding { Top = 10, Bottom = 10 },
                        },
                    };

                    linkFlow.AddLink("cancel", api.Logout, string.Empty);
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
                                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                                            Margin = new MarginPadding { Top = 5, Bottom = 5 },
                                        },
                                    },
                                },
                                panel = new UserGridPanel(api.LocalUser.Value)
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Action = RequestHide
                                },
                                dropdown = new UserDropdown { RelativeSizeAxes = Axes.X },
                            },
                        },
                    };

                    panel.Status.BindTo(api.LocalUser.Value.Status);
                    panel.Activity.BindTo(api.LocalUser.Value.Activity);

                    dropdown.Current.BindValueChanged(action =>
                    {
                        switch (action.NewValue)
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
                    }, true);
                    break;
            }

            if (form != null) GetContainingInputManager()?.ChangeFocus(form);
        });

        public override bool AcceptsFocus => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override void OnFocus(FocusEvent e)
        {
            if (form != null) GetContainingInputManager().ChangeFocus(form);
            base.OnFocus(e);
        }

        private class LoginForm : FillFlowContainer
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
                Children = new Drawable[]
                {
                    username = new OsuTextBox
                    {
                        PlaceholderText = "username",
                        RelativeSizeAxes = Axes.X,
                        Text = api?.ProvidedUsername ?? string.Empty,
                        TabbableContentContainer = this
                    },
                    password = new OsuPasswordTextBox
                    {
                        PlaceholderText = "password",
                        RelativeSizeAxes = Axes.X,
                        TabbableContentContainer = this,
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
                                    Text = "Sign in",
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
            }

            public override bool AcceptsFocus => true;

            protected override bool OnClick(ClickEvent e) => true;

            protected override void OnFocus(FocusEvent e)
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
                    if (Header is UserDropdownHeader h)
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
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = colours.Gray3;
                }

                protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableUserDropdownMenuItem(item);

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
                    set => statusIcon.FadeColour(value, 500, Easing.OutQuint);
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
                        Icon = FontAwesome.Regular.Circle,
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
