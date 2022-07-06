// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components.RadioButtons
{
    public class EditorRadioButton : OsuButton, IHasTooltip
    {
        /// <summary>
        /// Invoked when this <see cref="EditorRadioButton"/> has been selected.
        /// </summary>
        public Action<RadioButton> Selected;

        public readonly RadioButton Button;

        private Color4 defaultBackgroundColour;
        private Color4 defaultBubbleColour;
        private Color4 selectedBackgroundColour;
        private Color4 selectedBubbleColour;

        private Drawable icon;

        [Resolved(canBeNull: true)]
        private EditorBeatmap editorBeatmap { get; set; }

        public EditorRadioButton(RadioButton button)
        {
            Button = button;

            Text = button.Label;
            Action = button.Select;

            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            defaultBackgroundColour = colours.Gray3;
            defaultBubbleColour = defaultBackgroundColour.Darken(0.5f);
            selectedBackgroundColour = colours.BlueDark;
            selectedBubbleColour = selectedBackgroundColour.Lighten(0.5f);

            Content.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 2,
                Offset = new Vector2(0, 1),
                Colour = Color4.Black.Opacity(0.5f)
            };

            Add(icon = (Button.CreateIcon?.Invoke() ?? new Circle()).With(b =>
            {
                b.Blending = BlendingParameters.Additive;
                b.Anchor = Anchor.CentreLeft;
                b.Origin = Anchor.CentreLeft;
                b.Size = new Vector2(20);
                b.X = 10;
            }));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Button.Selected.ValueChanged += selected =>
            {
                updateSelectionState();
                if (selected.NewValue)
                    Selected?.Invoke(Button);
            };

            editorBeatmap?.HasTiming.BindValueChanged(hasTiming => Button.Selected.Disabled = !hasTiming.NewValue, true);

            Button.Selected.BindDisabledChanged(disabled => Enabled.Value = !disabled, true);
            updateSelectionState();
        }

        private void updateSelectionState()
        {
            if (!IsLoaded)
                return;

            BackgroundColour = Button.Selected.Value ? selectedBackgroundColour : defaultBackgroundColour;
            icon.Colour = Button.Selected.Value ? selectedBubbleColour : defaultBubbleColour;
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.CentreLeft,
            Anchor = Anchor.CentreLeft,
            X = 40f
        };

        public LocalisableString TooltipText => Enabled.Value ? string.Empty : "Add at least one timing point first!";
    }
}
