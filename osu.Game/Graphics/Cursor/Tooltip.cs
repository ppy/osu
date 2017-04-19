// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using System.Linq;

namespace osu.Game.Graphics.Cursor
{
    public class Tooltip : Container
    {
        private readonly Box tooltipBackground;
        private readonly OsuSpriteText text;

        private ScheduledDelegate show;
        private UserInputManager input;
        private IHasDisappearingTooltip disappearingTooltip;

        public const int DEFAULT_APPEAR_DELAY = 250;

        public string TooltipText {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
                if (string.IsNullOrEmpty(value))
                    Hide();
                else
                    Show();
            }
        }

        public IMouseState MouseState
        {
            set
            {
                if (value.Position != value.LastPosition && disappearingTooltip?.Disappear != false)
                {
                    show?.Cancel();
                    TooltipText = string.Empty;
                    IHasTooltip hasTooltip = input.HoveredDrawables.OfType<IHasTooltip>().FirstOrDefault();
                    if (hasTooltip != null)
                    {
                        IHasTooltipWithCustomDelay delayedTooltip = hasTooltip as IHasTooltipWithCustomDelay;
                        disappearingTooltip = hasTooltip as IHasDisappearingTooltip;
                        show = Scheduler.AddDelayed(() => TooltipText = hasTooltip.TooltipText, delayedTooltip?.TooltipDelay ?? DEFAULT_APPEAR_DELAY);
                    }
                }
            }
        }

        public Tooltip()
        {
            AlwaysPresent = true;
            Children = new[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(40),
                        Radius = 5,
                    },
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
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, UserInputManager input)
        {
            this.input = input;
            tooltipBackground.Colour = colour.Gray3;
        }

        protected override void Update()
        {
            if (disappearingTooltip?.Disappear == false)
                TooltipText = disappearingTooltip.TooltipText;
            else if (disappearingTooltip != null)
            {
                disappearingTooltip = null;
                TooltipText = string.Empty;
            }
        }
    }
}
