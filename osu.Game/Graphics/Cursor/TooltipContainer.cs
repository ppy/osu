// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using System.Linq;

namespace osu.Game.Graphics.Cursor
{
    public class TooltipContainer : Container
    {
        private readonly CursorContainer cursor;
        private readonly Tooltip tooltip;

        private ScheduledDelegate findTooltipTask;
        private UserInputManager inputManager;

        private const int default_appear_delay = 220;

        private IHasTooltip currentlyDisplayed;

        public TooltipContainer(CursorContainer cursor)
        {
            this.cursor = cursor;
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;
            Add(tooltip = new Tooltip { Alpha = 0 });
        }

        [BackgroundDependencyLoader]
        private void load(UserInputManager input)
        {
            inputManager = input;
        }

        protected override void Update()
        {
            if (tooltip.IsPresent)
            {
                if (currentlyDisplayed != null)
                    tooltip.TooltipText = currentlyDisplayed.TooltipText;

                //update the position of the displayed tooltip.
                tooltip.Position = ToLocalSpace(cursor.ActiveCursor.ScreenSpaceDrawQuad.Centre) + new Vector2(10);
            }
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            updateTooltipState(state);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnMouseMove(InputState state)
        {
            updateTooltipState(state);
            return base.OnMouseMove(state);
        }

        private void updateTooltipState(InputState state)
        {
            if (currentlyDisplayed?.Hovering != true)
            {
                if (currentlyDisplayed != null && !state.Mouse.HasMainButtonPressed)
                {
                    tooltip.Delay(150);
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
        }

        public class Tooltip : Container
        {
            private readonly Box background;
            private readonly OsuSpriteText text;

            public string TooltipText
            {
                set
                {
                    if (value == text.Text) return;

                    text.Text = value;
                    if (Alpha > 0)
                    {
                        AutoSizeDuration = 250;
                        background.FlashColour(OsuColour.Gray(0.4f), 1000, EasingTypes.OutQuint);
                    }
                    else
                        AutoSizeDuration = 0;
                }
            }

            public override bool HandleInput => false;

            private const float text_size = 16;

            public Tooltip()
            {
                AutoSizeEasing = EasingTypes.OutQuint;
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
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.9f,
                    },
                    text = new OsuSpriteText
                    {
                        TextSize = text_size,
                        Padding = new MarginPadding(5),
                        Font = @"Exo2.0-Regular",
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                background.Colour = colour.Gray3;
            }
        }
    }
}
