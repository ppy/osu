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
        private readonly Button button;
        public Action ExitAction
        {
            get => button.ExitAction;
            set => button.ExitAction = value;
        }

        public QuitButton()
        {
            OsuSpriteText text;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(20, 0);
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Text = "Hold to Quit",
                    Font = @"Exo2.0-Bold",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                button = new Button(text)
            };
            AutoSizeAxes = Axes.Both;
        }

        private class Button : CircularContainer
        {
            private readonly OsuSpriteText text;
            private SpriteIcon icon;
            private CircularProgress progress;

            public Action ExitAction { get; set; }

            private const int fade_duration = 200;
            private const int progress_duration = 1000;
            private const int text_display_time = 5000;

            public Button(OsuSpriteText text) => this.text = text;

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
                Scheduler.AddDelayed(() => text.FadeOut(fade_duration), text_display_time);
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                icon.ScaleTo(1.5f);
                text.FadeIn(fade_duration);
                progress.FillTo(1, progress_duration).OnComplete(cp => ExitAction());

                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                icon.ScaleTo(1f);
                Scheduler.AddDelayed(() => text.FadeOut(fade_duration), text_display_time);
                progress.FillTo(0, progress_duration / 4).OnComplete(cp => progress.Current.SetDefault());

                return base.OnMouseUp(state, args);
            }
        }
    }
}
