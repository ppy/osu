// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using System;
using System.Linq;

namespace osu.Game.Graphics.Cursor
{
    public class TooltipContainer : Container
    {
        private readonly CursorContainer cursor;
        private readonly Tooltip tooltip;

        private ScheduledDelegate show;
        private UserInputManager input;
        private IHasDisappearingTooltip disappearingTooltip;
        private IHasTooltip hasTooltip;

        public const int DEFAULT_APPEAR_DELAY = 250;

        public TooltipContainer(CursorContainer cursor)
        {
            this.cursor = cursor;
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;
            Add(tooltip = new Tooltip());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, UserInputManager input)
        {
            this.input = input;
        }

        protected override void Update()
        {
            if (tooltip?.IsPresent == true)
                tooltip.TooltipText = hasTooltip?.TooltipText;
            else if (disappearingTooltip?.Disappear == true && show?.Completed == true)
            {
                disappearingTooltip = null;
                tooltip.TooltipText = string.Empty;
            }
        }

        protected override bool OnMouseMove(InputState state)
        {
            if (((hasTooltip as Drawable)?.Hovering != true && disappearingTooltip?.Disappear != false) || show?.Completed != true)
            {
                show?.Cancel();
                tooltip.TooltipText = string.Empty;
                hasTooltip = input.HoveredDrawables.OfType<IHasTooltip>().FirstOrDefault();
                if (hasTooltip != null)
                {
                    IHasTooltipWithCustomDelay delayedTooltip = hasTooltip as IHasTooltipWithCustomDelay;
                    disappearingTooltip = hasTooltip as IHasDisappearingTooltip;
                    show = Scheduler.AddDelayed(delegate
                    {
                        tooltip.TooltipText = hasTooltip.TooltipText;
                        tooltip.Position = new Vector2(state.Mouse.Position.X, Math.Min(cursor.ActiveCursor.BoundingBox.Bottom, state.Mouse.Position.Y + cursor.ActiveCursor.DrawHeight));
                    }, delayedTooltip?.TooltipDelay ?? DEFAULT_APPEAR_DELAY);
                }
            }

            return base.OnMouseMove(state);
        }

        public class Tooltip : Container
        {
            private readonly Box tooltipBackground;
            private readonly OsuSpriteText text;

            public string TooltipText
            {
                set
                {
                    text.Text = value;
                    if (string.IsNullOrEmpty(value) && !Hovering)
                        Hide();
                    else
                        Show();
                }
            }

            public override bool HandleInput => false;

            public Tooltip()
            {

                AutoSizeAxes = Axes.Both;
                CornerRadius = 5;
                Masking = true;
                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(40),
                    Radius = 5,
                };
                Children = new Drawable[]
                {
                    tooltipBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    text = new OsuSpriteText
                    {
                        Padding = new MarginPadding(3),
                        Font = @"Exo2.0-Regular",
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                tooltipBackground.Colour = colour.Gray3;
            }
        }
    }
}
