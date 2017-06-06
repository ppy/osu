﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.Cursor
{
    public class OsuTooltipContainer : TooltipContainer
    {
        protected override Tooltip CreateTooltip() => new OsuTooltip();

        public OsuTooltipContainer(CursorContainer cursor) : base(cursor)
        {
        }

        public class OsuTooltip : Tooltip
        {
            private readonly Box background;
            private readonly OsuSpriteText text;
            private bool instantMovement = true;

            public override string TooltipText
            {
                set
                {
                    if (value == text.Text) return;

                    text.Text = value;
                    if (IsPresent)
                    {
                        AutoSizeDuration = 250;
                        background.FlashColour(OsuColour.Gray(0.4f), 1000, EasingTypes.OutQuint);
                    }
                    else
                        AutoSizeDuration = 0;
                }
            }

            private const float text_size = 16;

            public OsuTooltip()
            {
                AutoSizeEasing = EasingTypes.OutQuint;

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

            protected override void PopIn()
            {
                instantMovement |= !IsPresent;
                FadeIn(500, EasingTypes.OutQuint);
            }

            protected override void PopOut()
            {
                using (BeginDelayedSequence(150))
                    FadeOut(500, EasingTypes.OutQuint);
            }

            public override void Move(Vector2 pos)
            {
                if (instantMovement)
                {
                    Position = pos;
                    instantMovement = false;
                }
                else
                {
                    MoveTo(pos, 200, EasingTypes.OutQuint);
                }
            }
        }
    }
}
