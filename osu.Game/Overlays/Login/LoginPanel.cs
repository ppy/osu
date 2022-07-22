// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Users;
using osuTK;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays.Login
{
    public class LoginPanel : FillFlowContainer
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

        public LoginPanel()
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

            if (form != null)
                ScheduleAfterChildren(() => GetContainingInputManager()?.ChangeFocus(form));
        });

        public override bool AcceptsFocus => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override void OnFocus(FocusEvent e)
        {
            if (form != null) GetContainingInputManager().ChangeFocus(form);
            base.OnFocus(e);
        }
    }
}
