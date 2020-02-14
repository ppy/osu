// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineButton : CompositeDrawable
    {
        public Action Action;
        public readonly BindableBool Enabled = new BindableBool(true);

        public IconUsage Icon
        {
            get => button.Icon;
            set => button.Icon = value;
        }

        private readonly IconButton button;

        public TimelineButton()
        {
            InternalChild = button = new TimelineIconButton { Action = () => Action?.Invoke() };

            button.Enabled.BindTo(Enabled);
            Width = button.Width;
        }

        protected override void Update()
        {
            base.Update();

            button.Size = new Vector2(button.Width, DrawHeight);
        }

        private class TimelineIconButton : IconButton
        {
            public TimelineIconButton()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                IconColour = OsuColour.Gray(0.35f);
                IconHoverColour = Color4.White;
                HoverColour = OsuColour.Gray(0.25f);
                FlashColour = OsuColour.Gray(0.5f);
            }

            private ScheduledDelegate repeatSchedule;

            /// <summary>
            /// The initial delay before mouse down repeat begins.
            /// </summary>
            private const int repeat_initial_delay = 250;

            /// <summary>
            /// The delay between mouse down repeats after the initial repeat.
            /// </summary>
            private const int repeat_tick_rate = 70;

            protected override bool OnClick(ClickEvent e)
            {
                // don't actuate a click since we are manually handling repeats.
                return true;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Left)
                {
                    Action clickAction = () => base.OnClick(new ClickEvent(e.CurrentState, e.Button));

                    // run once for initial down
                    clickAction();

                    Scheduler.Add(repeatSchedule = new ScheduledDelegate(clickAction, Clock.CurrentTime + repeat_initial_delay, repeat_tick_rate));
                }

                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                repeatSchedule?.Cancel();
                base.OnMouseUp(e);
            }
        }
    }
}
