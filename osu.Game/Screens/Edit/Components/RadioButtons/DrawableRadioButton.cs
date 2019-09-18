// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components.RadioButtons
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
                Blending = BlendingParameters.Additive,
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

            button.Selected.ValueChanged += selected =>
            {
                updateSelectionState();
                if (selected.NewValue)
                    Selected?.Invoke(button);
            };

            updateSelectionState();
        }

        private void updateSelectionState()
        {
            if (!IsLoaded)
                return;

            BackgroundColour = button.Selected.Value ? selectedBackgroundColour : defaultBackgroundColour;
            bubble.Colour = button.Selected.Value ? selectedBubbleColour : defaultBubbleColour;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (button.Selected.Value)
                return true;

            if (!Enabled.Value)
                return true;

            button.Selected.Value = true;

            return base.OnClick(e);
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
