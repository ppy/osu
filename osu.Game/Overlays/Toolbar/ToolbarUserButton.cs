// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarUserButton : ToolbarOverlayToggleButton
    {
        private UpdateableAvatar avatar = null!;

        private IBindable<APIUser> localUser = null!;

        private LoadingSpinner spinner = null!;

        private SpriteIcon failingIcon = null!;

        private IBindable<APIState> apiState = null!;

        public ToolbarUserButton()
        {
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IAPIProvider api, LoginOverlay? login)
        {
            Flow.Add(new Container
            {
                Masking = true,
                CornerRadius = 4,
                Size = new Vector2(32),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 4,
                    Colour = Color4.Black.Opacity(0.1f),
                },
                Children = new Drawable[]
                {
                    avatar = new UpdateableAvatar(isInteractive: false)
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    spinner = new LoadingLayer(dimBackground: true, withBox: false, blockInput: false)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                    },
                    failingIcon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,
                        Size = new Vector2(0.3f),
                        Icon = FontAwesome.Solid.ExclamationTriangle,
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.YellowLight,
                    },
                }
            });

            apiState = api.State.GetBoundCopy();
            apiState.BindValueChanged(onlineStateChanged, true);

            localUser = api.LocalUser.GetBoundCopy();
            localUser.BindValueChanged(userChanged, true);

            StateContainer = login;
        }

        private void userChanged(ValueChangedEvent<APIUser> user) => Schedule(() =>
        {
            Text = user.NewValue.Username;
            avatar.User = user.NewValue;
        });

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            failingIcon.FadeTo(state.NewValue == APIState.Failing ? 1 : 0, 200, Easing.OutQuint);

            switch (state.NewValue)
            {
                case APIState.Connecting:
                    TooltipText = ToolbarStrings.Connecting;
                    spinner.Show();
                    break;

                case APIState.Failing:
                    TooltipText = ToolbarStrings.AttemptingToReconnect;
                    spinner.Show();
                    break;

                case APIState.Offline:
                case APIState.Online:
                    TooltipText = string.Empty;
                    spinner.Hide();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }
}
