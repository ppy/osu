// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A Checkbox styled to be placed in line with an <see cref="OsuTabControl{T}"/>
    /// </summary>
    public class OsuTabControlCheckbox : Checkbox
    {
        private readonly Box box;
        private readonly SpriteText text;
        private readonly SpriteIcon icon;

        private Color4? accentColour;

        public Color4 AccentColour
        {
            get => accentColour.GetValueOrDefault();
            set
            {
                accentColour = value;

                if (Current.Value)
                {
                    text.Colour = AccentColour;
                    icon.Colour = AccentColour;
                }

                updateFade();
            }
        }

        public string Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        private const float transition_length = 500;

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
                        text = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                        icon = new SpriteIcon
                        {
                            Size = new Vector2(14),
                            Icon = FontAwesome.Regular.Circle,
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
                },
                new HoverClickSounds()
            };

            Current.ValueChanged += selected =>
            {
                icon.Icon = selected.NewValue ? FontAwesome.Regular.CheckCircle : FontAwesome.Regular.Circle;
                text.Font = text.Font.With(weight: selected.NewValue ? FontWeight.Bold : FontWeight.Medium);
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateFade();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!Current.Value)
                updateFade();

            base.OnHoverLost(e);
        }

        private void updateFade()
        {
            box.FadeTo(IsHovered ? 1 : 0, transition_length, Easing.OutQuint);
            text.FadeColour(IsHovered ? Color4.White : AccentColour, transition_length, Easing.OutQuint);
        }
    }
}
