// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Compose.RadioButtons
{
    public class DrawableRadioButton : TriangleButton
    {
        private static readonly Color4 default_background_colour = OsuColour.FromHex("333");
        private static readonly Color4 default_bubble_colour = default_background_colour.Darken(0.5f);
        private static readonly Color4 selected_background_colour = OsuColour.FromHex("1188aa");
        private static readonly Color4 selected_bubble_colour = selected_background_colour.Lighten(0.5f);

        /// <summary>
        /// Invoked when this <see cref="DrawableRadioButton"/> has been selected.
        /// </summary>
        public Action<DrawableRadioButton> Selected;

        private Drawable bubble;

        public DrawableRadioButton(RadioButton button)
        {
            Text = button.Text;
            Action = button.Action;

            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Triangles.Alpha = 0;
            BackgroundColour = default_background_colour;

            Content.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 2,
                Offset = new Vector2(0, 1),
                Colour = Color4.Black.Opacity(0.5f)
            };

            Add(bubble = new CircularContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Scale = new Vector2(0.5f),
                X = 10,
                Masking = true,
                Colour = default_bubble_colour,
                Blending = BlendingMode.Additive,
                Child = new Box { RelativeSizeAxes = Axes.Both }
            });
        }

        private bool isSelected;

        public void Deselect()
        {
            if (!isSelected)
                return;
            isSelected = false;

            BackgroundColour = default_background_colour;
            bubble.Colour = default_bubble_colour;
        }

        public void Select()
        {
            if (isSelected)
                return;
            isSelected = true;
            Selected?.Invoke(this);

            BackgroundColour = selected_background_colour;
            bubble.Colour = selected_bubble_colour;
        }

        protected override bool OnClick(InputState state)
        {
            if (isSelected)
                return true;

            if (!Enabled)
                return true;

            Select();

            return base.OnClick(state);
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.CentreLeft,
            Anchor = Anchor.CentreLeft,
            X = 40f
        };
    }
}
