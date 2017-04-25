// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A Checkbox styled to be placed in line with an <see cref="OsuTabControl{T}"/>
    /// </summary>
    public class OsuTabControlCheckbox : Checkbox
    {
        private readonly Box box;
        private readonly SpriteText text;
        private readonly TextAwesome icon;

        private Color4? accentColour;
        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;

                if (Current)
                {
                    text.Colour = AccentColour;
                    icon.Colour = AccentColour;
                }
            }
        }

        public string Text
        {
            get { return text.Text; }
            set { text.Text = value; }
        }

        private const float transition_length = 500;

        private void fadeIn()
        {
            box.FadeIn(transition_length, EasingTypes.OutQuint);
            text.FadeColour(Color4.White, transition_length, EasingTypes.OutQuint);
        }

        private void fadeOut()
        {
            box.FadeOut(transition_length, EasingTypes.OutQuint);
            text.FadeColour(AccentColour, transition_length, EasingTypes.OutQuint);
        }

        protected override bool OnHover(InputState state)
        {
            fadeIn();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (!Current)
                fadeOut();

            base.OnHoverLost(state);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }

        public OsuTabControlCheckbox()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Top = 5, Bottom = 5, },
                    Spacing = new Vector2(5f, 0f),
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                        },
                        icon = new TextAwesome
                        {
                            TextSize = 14,
                            Icon = FontAwesome.fa_circle_o,
                            Shadow = true,
                        },
                    },
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

            Current.ValueChanged += v =>
            {
                if (v)
                {
                    fadeIn();
                    icon.Icon = FontAwesome.fa_check_circle_o;
                }
                else
                {
                    fadeOut();
                    icon.Icon = FontAwesome.fa_circle_o;
                }
            };
        }
    }
}
