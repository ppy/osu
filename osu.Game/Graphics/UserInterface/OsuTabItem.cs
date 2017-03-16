// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface.Tab;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabItem<T> : TabItem<T>
    {
        private SpriteText text;
        private Box box;
        private Color4 fadeColour;

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
                if (value)
                    fadeActive();
                else
                    fadeInactive();
                base.Active = value;
            }
        }

        private void fadeActive()
        {
            box.FadeIn(300);
            text.FadeColour(Color4.White, 300);
        }

        private void fadeInactive()
        {
            box.FadeOut(300);
            text.FadeColour(fadeColour, 300);
        }

        protected override bool OnHover(InputState state) {
            if (!Active)
                fadeActive();
            return true;
        }

        protected override void OnHoverLost(InputState state) {
            if (!Active)
                fadeInactive();
        }

        public OsuTabItem()
        {
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Margin = new MarginPadding(5),
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
            if (typeof(T) == typeof(SortMode))
            {
                fadeColour = colours.GreenLight;
                if (!Active)
                    text.Colour = colours.GreenLight;
            }
            else
            {
                fadeColour = colours.Blue;
                if (!Active)
                    text.Colour = colours.Blue;
            }
        }
    }
}
