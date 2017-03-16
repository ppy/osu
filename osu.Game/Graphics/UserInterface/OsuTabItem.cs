// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabItem<T> : TabItem<T>
    {
        private SpriteText text;
        private Box box;
        
        private Color4? accentColour;
        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                if (!Active)
                    text.Colour = value;
            }
        }

        public new T Value
        {
            get { return base.Value; }
            set
            {
                base.Value = value;
                text.Text = (value as Enum)?.GetDescription();
            }
        }

        public override bool Active
        {
            get { return base.Active; }
            set
            {
                if (Active == value) return;

                if (value)
                    fadeActive();
                else
                    fadeInactive();
                base.Active = value;
            }
        }

        private const float transition_length = 500;

        private void fadeActive()
        {
            box.FadeIn(transition_length, EasingTypes.OutQuint);
            text.FadeColour(Color4.White, transition_length, EasingTypes.OutQuint);
        }

        private void fadeInactive()
        {
            box.FadeOut(transition_length, EasingTypes.OutQuint);
            text.FadeColour(AccentColour, transition_length, EasingTypes.OutQuint);
        }

        protected override bool OnHover(InputState state)
        {
            if (!Active)
                fadeActive();
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            if (!Active)
                fadeInactive();
        }

        public OsuTabItem()
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Margin = new MarginPadding(5),
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    TextSize = 14,
                    Font = @"Exo2.0-Bold", // Font should only turn bold when active?
                },
                box = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Alpha = 0,
                    Colour = Color4.White,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }
    }
}
