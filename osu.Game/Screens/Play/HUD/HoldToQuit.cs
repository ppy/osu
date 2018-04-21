// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Screens.Play.HUD
{
    public class HoldToQuit : Container
    {
        private readonly OsuSpriteText text;
        private readonly HoldToQuitButton button;

        public HoldToQuit()
        {
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Text = "Hold to Quit",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                button = new HoldToQuitButton(text)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight
                }
            };
            AutoSizeAxes = Axes.Both;
        }

        private class HoldToQuitButton : CircularContainer
        {
            private readonly OsuSpriteText text;
            private SpriteIcon icon;
            private CircularProgress progress;

            private Action exitAction;
            private readonly Scheduler scheduler;
            private ScheduledDelegate scheduledExitAction;

            private const int fade_duration = 200;

            public HoldToQuitButton(OsuSpriteText text)
            {
                this.text = text;
                scheduler = new Scheduler();

                // TODO provide action
                exitAction = () => Thread.Sleep(1);
            }

            private void hideText() => scheduler.AddDelayed(() => text.FadeOut(fade_duration), 5000);

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
                    progress = new CircularProgress { RelativeSizeAxes = Axes.Both, InnerRadius = 0.1f, Current = { Value = 1 } }
                });
                progress.Hide();
                hideText();
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                icon.ScaleTo(1.5f);
                text.FadeIn(fade_duration);
                progress.FadeIn(1000);
                scheduledExitAction = scheduler.AddDelayed(exitAction, 1000);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                icon.ScaleTo(1f);
                hideText();
                if (scheduledExitAction != null && !scheduledExitAction.Completed)
                    scheduledExitAction.Cancel();
                progress.FadeOut(fade_duration);
                return base.OnMouseUp(state, args);
            }

            protected override void Update()
            {
                scheduler.Update();
                base.Update();
            }
        }
    }
}
