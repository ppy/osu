﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Settings;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Login
{
    public partial class LoginPanel : Container
    {
        private bool bounding = true;

        private Drawable? form;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private UserDropdown? dropdown;

        /// <summary>
        /// Called to request a hide of a parent displaying this container.
        /// </summary>
        public Action? RequestHide;

        private IBindable<APIUser> user = null!;
        private readonly Bindable<UserStatus?> status = new Bindable<UserStatus?>();

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);

            user = api.LocalUser.GetBoundCopy();
            user.BindValueChanged(u =>
            {
                status.UnbindBindings();
                status.BindTo(u.NewValue.Status);
            }, true);

            status.BindValueChanged(e => updateDropdownCurrent(e.NewValue), true);
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            form = null;

            switch (state.NewValue)
            {
                case APIState.Offline:
                    Child = form = new LoginForm
                    {
                        RequestHide = RequestHide
                    };
                    break;

                case APIState.RequiresSecondFactorAuth:
                    Child = form = new SecondFactorAuthForm();
                    break;

                case APIState.Failing:
                case APIState.Connecting:
                    LinkFlowContainer linkFlow;

                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS },
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0f, SettingsSection.ITEM_SPACING),
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
                                Text = state.NewValue == APIState.Failing ? ToolbarStrings.AttemptingToReconnect : ToolbarStrings.Connecting,
                            },
                        },
                    };

                    linkFlow.AddLink(Resources.Localisation.Web.CommonStrings.ButtonsCancel.ToLower(), api.Logout, string.Empty);
                    break;

                case APIState.Online:
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS },
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0f, SettingsSection.ITEM_SPACING),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = LoginPanelStrings.SignedIn,
                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                            },
                            new UserRankPanel(api.LocalUser.Value)
                            {
                                RelativeSizeAxes = Axes.X,
                                Action = RequestHide
                            },
                            dropdown = new UserDropdown { RelativeSizeAxes = Axes.X },
                        },
                    };

                    dropdown.Current.BindValueChanged(action =>
                    {
                        switch (action.NewValue)
                        {
                            case UserAction.Online:
                                api.LocalUser.Value.Status.Value = UserStatus.Online;
                                dropdown.StatusColour = colours.Green;
                                break;

                            case UserAction.DoNotDisturb:
                                api.LocalUser.Value.Status.Value = UserStatus.DoNotDisturb;
                                dropdown.StatusColour = colours.Red;
                                break;

                            case UserAction.AppearOffline:
                                api.LocalUser.Value.Status.Value = UserStatus.Offline;
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

        private void updateDropdownCurrent(UserStatus? status)
        {
            if (dropdown == null)
                return;

            switch (status)
            {
                case UserStatus.Online:
                    dropdown.Current.Value = UserAction.Online;
                    break;

                case UserStatus.DoNotDisturb:
                    dropdown.Current.Value = UserAction.DoNotDisturb;
                    break;

                case UserStatus.Offline:
                    dropdown.Current.Value = UserAction.AppearOffline;
                    break;
            }
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override void OnFocus(FocusEvent e)
        {
            if (form != null) GetContainingInputManager().ChangeFocus(form);
            base.OnFocus(e);
        }
    }
}
