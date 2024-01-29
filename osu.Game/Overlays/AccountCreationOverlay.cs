// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays.AccountCreation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class AccountCreationOverlay : OsuFocusedOverlayContainer
    {
        private const float transition_time = 400;

        private ScreenWelcome welcomeScreen;

        public AccountCreationOverlay()
        {
            Size = new Vector2(620, 450);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(apiStateChanged, true);

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.6f,
                        },
                        new DelayedLoadWrapper(new AccountCreationBackground(), 0),
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.6f,
                            AutoSizeDuration = transition_time,
                            AutoSizeEasing = Easing.OutQuint,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.9f,
                                },
                                new ScreenStack(welcomeScreen = new ScreenWelcome())
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void PopIn()
        {
            this.FadeIn(transition_time, Easing.OutQuint);

            if (welcomeScreen.GetChildScreen() != null)
                welcomeScreen.MakeCurrent();

            // there might be a stale scheduled hide from a previous API state change.
            // cancel it here so that the overlay is not hidden again after one frame.
            scheduledHide?.Cancel();
            scheduledHide = null;
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(100);
        }

        private ScheduledDelegate scheduledHide;

        private void apiStateChanged(ValueChangedEvent<APIState> state)
        {
            switch (state.NewValue)
            {
                case APIState.Offline:
                case APIState.Failing:
                    break;

                case APIState.Connecting:
                case APIState.RequiresSecondFactorAuth:
                    break;

                case APIState.Online:
                    scheduledHide?.Cancel();
                    scheduledHide = Schedule(Hide);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
