// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
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

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();
        private readonly Bindable<UserStatus> configUserStatus = new Bindable<UserStatus>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

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

            config.BindWith(OsuSetting.UserOnlineStatus, configUserStatus);
            configUserStatus.BindValueChanged(e => updateDropdownCurrent(e.NewValue), true);

            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);
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

                    updateDropdownCurrent(configUserStatus.Value);
                    dropdown.Current.BindValueChanged(action =>
                    {
                        switch (action.NewValue)
                        {
                            case UserAction.Online:
                                configUserStatus.Value = UserStatus.Online;
                                dropdown.StatusColour = colours.Green;
                                break;

                            case UserAction.DoNotDisturb:
                                configUserStatus.Value = UserStatus.DoNotDisturb;
                                dropdown.StatusColour = colours.Red;
                                break;

                            case UserAction.AppearOffline:
                                configUserStatus.Value = UserStatus.Offline;
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
                ScheduleAfterChildren(() => GetContainingFocusManager()?.ChangeFocus(form));
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
            if (form != null) GetContainingFocusManager()!.ChangeFocus(form);
            base.OnFocus(e);
        }
    }
}
