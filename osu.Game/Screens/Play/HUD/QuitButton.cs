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
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Screens.Play.HUD
{
    public class QuitButton : FillFlowContainer
    {
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => button.ReceiveMouseInputAt(screenSpacePos);

        private readonly Button button;

        public Action ExitAction
        {
            get => button.ExitAction;
            set => button.ExitAction = value;
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
            return base.OnHover(state);
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

            bool stayVisible = text.Alpha > 0 || button.Progress > 0 || IsHovered;

            Alpha = stayVisible ? 1 : Interpolation.ValueAt(elapsed, Alpha, MathHelper.Clamp(1 - adjust, 0.04f, 1), 0, 200, Easing.OutQuint);
        }

        private class Button : CircularContainer
        {
            private SpriteIcon icon;
            private CircularProgress progress;
            private Circle innerCircle;

            private bool triggered;

            public Action ExitAction { get; set; }

            public double Progress => progress.Current.Value;

            private const int fade_duration = 200;
            private const int progress_duration = 600;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Masking = true;
                Size = new Vector2(60);
                AddRange(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Gray1,
                        Alpha = 0.5f,
                    },
                    progress = new CircularProgress
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
                });
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                if (state.Mouse.Buttons.Count > 1 || triggered)
                    return true;

                icon.ScaleTo(1.4f, progress_duration);
                progress.FillTo(1, progress_duration, Easing.OutSine).OnComplete(_ =>
                {
                    innerCircle.ScaleTo(0, 100).Then().FadeOut().ScaleTo(1).FadeIn(500);
                    triggered = true;
                    ExitAction();
                });

                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                if (state.Mouse.Buttons.Count > 0 || triggered)
                    return true;

                icon.ScaleTo(1, 800, Easing.OutElastic);
                progress.FillTo(0, progress_duration, Easing.OutQuint).OnComplete(cp => progress.Current.SetDefault());

                return base.OnMouseUp(state, args);
            }
        }
    }
}
