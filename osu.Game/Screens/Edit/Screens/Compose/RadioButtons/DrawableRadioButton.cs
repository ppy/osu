// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Compose.RadioButtons
{
    public class DrawableRadioButton : TriangleButton
    {
        /// <summary>
        /// Invoked when this <see cref="DrawableRadioButton"/> has been selected.
        /// </summary>
        public Action<RadioButton> Selected;

        private Color4 defaultBackgroundColour;
        private Color4 defaultBubbleColour;
        private Color4 selectedBackgroundColour;
        private Color4 selectedBubbleColour;

        private readonly Drawable bubble;
        private readonly RadioButton button;

        public DrawableRadioButton(RadioButton button)
        {
            this.button = button;

            Text = button.Text;
            Action = button.Action;

            RelativeSizeAxes = Axes.X;

            bubble = new CircularContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Scale = new Vector2(0.5f),
                X = 10,
                Masking = true,
                Blending = BlendingMode.Additive,
                Child = new Box { RelativeSizeAxes = Axes.Both }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            defaultBackgroundColour = colours.Gray3;
            defaultBubbleColour = defaultBackgroundColour.Darken(0.5f);
            selectedBackgroundColour = colours.BlueDark;
            selectedBubbleColour = selectedBackgroundColour.Lighten(0.5f);

            Triangles.Alpha = 0;

            Content.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 2,
                Offset = new Vector2(0, 1),
                Colour = Color4.Black.Opacity(0.5f)
            };

            Add(bubble);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            button.Selected.ValueChanged += v =>
            {
                updateSelectionState();
                if (v)
                    Selected?.Invoke(button);
            };

            updateSelectionState();
        }

        private void updateSelectionState()
        {
            if (!IsLoaded)
                return;

            BackgroundColour = button.Selected ? selectedBackgroundColour : defaultBackgroundColour;
            bubble.Colour = button.Selected ? selectedBubbleColour : defaultBubbleColour;
        }

        protected override bool OnClick(InputState state)
        {
            if (button.Selected)
                return true;

            if (!Enabled)
                return true;

            button.Selected.Value = true;

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
