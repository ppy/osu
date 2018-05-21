// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Screens.Play.HUD
{
    public class QuitButton : FillFlowContainer
    {
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => button.ReceiveMouseInputAt(screenSpacePos);

        private readonly Button button;

        public Action Action
        {
            set => button.Action = value;
        }

        private readonly OsuSpriteText text;

        public QuitButton()
        {
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(20, 0);
            Margin = new MarginPadding(10);
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Text = "hold to quit",
                    Font = @"Exo2.0-Bold",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                button = new Button()
            };
            AutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            text.FadeInFromZero(500, Easing.OutQuint).Delay(1500).FadeOut(500, Easing.OutQuint);
            base.LoadComplete();
        }

        protected override bool OnHover(InputState state)
        {
            text.FadeIn(500, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            text.FadeOut(500, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        protected override void Update()
        {
            base.Update();

            float adjust = Vector2.Distance(GetContainingInputManager().CurrentState.Mouse.NativeState.Position, button.ScreenSpaceDrawQuad.Centre) / 200;
            double elapsed = MathHelper.Clamp(Clock.ElapsedFrameTime, 0, 1000);

            bool stayVisible = text.Alpha > 0 || button.Progress.Value > 0 || IsHovered;

            Alpha = stayVisible ? 1 : Interpolation.ValueAt(elapsed, Alpha, MathHelper.Clamp(1 - adjust, 0.04f, 1), 0, 200, Easing.OutQuint);
        }

        private class Button : HoldToCofirmContainer
        {
            private SpriteIcon icon;
            private CircularProgress circularProgress;
            private Circle innerCircle;

            protected override bool AllowMultipleFires => true;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Size = new Vector2(60);

                Child = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray1,
                            Alpha = 0.5f,
                        },
                        circularProgress = new CircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            InnerRadius = 1
                        },
                        innerCircle = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray1,
                            Size = new Vector2(0.9f),
                        },
                        icon = new SpriteIcon
                        {
                            Shadow = false,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(15),
                            Icon = FontAwesome.fa_close
                        },
                    }
                };

                bind();
            }

            private void bind()
            {
                circularProgress.Current.BindTo(Progress);
                Progress.ValueChanged += v => icon.Scale = new Vector2(1 + (float)v * 0.4f);
            }

            private bool pendingAnimation;

            protected override void Confirm()
            {
                base.Confirm();

                // temporarily unbind as to not look weird during flash animation.
                Progress.UnbindAll();
                pendingAnimation = true;

                innerCircle.ScaleTo(0, 100)
                           .Then().FadeOut().ScaleTo(1).FadeIn(500)
                           .OnComplete(a => circularProgress.FadeOut(100).OnComplete(_ =>
                           {
                               bind();
                               circularProgress.FadeIn();
                               pendingAnimation = false;
                           }));
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                if (!pendingAnimation && state.Mouse.Buttons.Count == 1)
                    BeginConfirm();
                return true;
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                if (state.Mouse.Buttons.Count == 0)
                    AbortConfirm();
                return true;
            }
        }
    }
}
