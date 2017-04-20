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

        private ScheduledDelegate findTooltipTask;
        private UserInputManager inputManager;

        private const int default_appear_delay = 250;

        private IHasTooltip currentlyDisplayed;

        private IMouseState lastState;

        public TooltipContainer(CursorContainer cursor)
        {
            this.cursor = cursor;
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;
            Add(tooltip = new Tooltip { Alpha = 0 });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, UserInputManager input)
        {
            this.inputManager = input;
        }

        protected override void Update()
        {
            if (tooltip.IsPresent && lastState != null)
            {
                if (currentlyDisplayed != null)
                    tooltip.TooltipText = currentlyDisplayed.TooltipText;

                //update the position of the displayed tooltip.
                tooltip.Position = new Vector2(
                    lastState.Position.X,
                    Math.Min(cursor.ActiveCursor.BoundingBox.Bottom, lastState.Position.Y + cursor.ActiveCursor.DrawHeight));
            }
        }

        protected override bool OnMouseMove(InputState state)
        {
            lastState = state.Mouse;

            if (currentlyDisplayed?.Hovering != true)
            {
                if (currentlyDisplayed != null)
                {
                    tooltip.Delay(100);
                    tooltip.FadeOut(500, EasingTypes.OutQuint);
                    currentlyDisplayed = null;
                }

                findTooltipTask?.Cancel();
                findTooltipTask = Scheduler.AddDelayed(delegate
                {
                    var tooltipTarget = inputManager.HoveredDrawables.OfType<IHasTooltip>().FirstOrDefault();

                    if (tooltipTarget == null) return;

                    tooltip.TooltipText = tooltipTarget.TooltipText;
                    tooltip.FadeIn(500, EasingTypes.OutQuint);

                    currentlyDisplayed = tooltipTarget;
                }, (1 - tooltip.Alpha) * default_appear_delay);
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
                        Padding = new MarginPadding(5),
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
