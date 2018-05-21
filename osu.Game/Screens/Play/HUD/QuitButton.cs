// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
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

        OsuSpriteText text;

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

        private class Button : CircularContainer
        {
            private SpriteIcon icon;
            private CircularProgress progress;

            public Action ExitAction { get; set; }

            private const int fade_duration = 200;
            private const int progress_duration = 1000;

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
                        Alpha = 0.8f,
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(15),
                        Icon = FontAwesome.fa_close
                    },
                    progress = new CircularProgress { RelativeSizeAxes = Axes.Both, InnerRadius = 0.1f }
                });
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                icon.ScaleTo(1.5f);
                progress.FillTo(1, progress_duration).OnComplete(cp => ExitAction());

                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                icon.ScaleTo(1f);
                progress.FillTo(0, progress_duration / 4f).OnComplete(cp => progress.Current.SetDefault());

                return base.OnMouseUp(state, args);
            }
        }
    }
}
