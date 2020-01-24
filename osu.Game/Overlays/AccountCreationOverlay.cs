// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays.AccountCreation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class AccountCreationOverlay : OsuFocusedOverlayContainer, IOnlineComponent
    {
        private const float transition_time = 400;

        private ScreenWelcome welcomeScreen;

        public AccountCreationOverlay()
        {
            Size = new Vector2(620, 450);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IAPIProvider api)
        {
            api.Register(this);

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
            base.PopIn();
            this.FadeIn(transition_time, Easing.OutQuint);

            if (welcomeScreen.GetChildScreen() != null)
                welcomeScreen.MakeCurrent();
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(100);
        }

        public void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                case APIState.Failing:
                    break;

                case APIState.Connecting:
                    break;

                case APIState.Online:
                    Hide();
                    break;
            }
        }
    }
}
