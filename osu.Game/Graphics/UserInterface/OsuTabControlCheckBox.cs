// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A checkbox styled to be placed in line with an <see cref="OsuTabControl"/>
    /// </summary>
    public class OsuTabControlCheckBox : CheckBox
    {
        private const float transition_length = 500;

        public event EventHandler<CheckBoxState> Action;

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;

                if (State == CheckBoxState.Unchecked)
                {
                    text.Colour = accentColour;
                    icon.Colour = accentColour;
                }
            }
        }

        public string Text
        {
            get { return text.Text; }
            set { text.Text = value; }
        }

        private Box box;
        private SpriteText text;
        private TextAwesome icon;

        private void fadeIn()
        {
            box.FadeIn(transition_length, EasingTypes.OutQuint);
            text.FadeColour(Color4.White, transition_length, EasingTypes.OutQuint);
        }

        private void fadeOut()
        {
            box.FadeOut(transition_length, EasingTypes.OutQuint);
            text.FadeColour(accentColour, transition_length, EasingTypes.OutQuint);
        }

        protected override void OnChecked()
        {
            fadeIn();
            icon.Icon = FontAwesome.fa_check_circle_o;
            Action?.Invoke(this, State);
        }

        protected override void OnUnchecked()
        {
            fadeOut();
            icon.Icon = FontAwesome.fa_circle_o;
            Action?.Invoke(this, State);
        }

        protected override bool OnHover(InputState state)
        {
            fadeIn();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (State == CheckBoxState.Unchecked)
                fadeOut();

            base.OnHoverLost(state);
        }

        public OsuTabControlCheckBox()
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
        }
    }
}
