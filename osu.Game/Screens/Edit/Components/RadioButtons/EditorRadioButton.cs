// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components.RadioButtons
{
    public partial class EditorRadioButton : OsuButton, IHasTooltip
    {
        /// <summary>
        /// Whether this <see cref="EditorRadioButton"/> is selected.
        /// Disable this bindable to disable the button.
        /// </summary>
        public readonly BindableBool Selected = new BindableBool();

        /// <summary>
        /// A function which creates a drawable icon to represent this item. If null, a sane default should be used.
        /// </summary>
        public readonly Func<Drawable?>? CreateIcon;

        private readonly Action? action;

        private Color4 defaultBackgroundColour;
        private Color4 defaultIconColour;
        private Color4 selectedBackgroundColour;
        private Color4 selectedIconColour;

        private Drawable icon = null!;

        public EditorRadioButton(string label, Action? action, Func<Drawable?>? createIcon = null)
        {
            Text = label;
            CreateIcon = createIcon;
            this.action = action;
            Action = Select;

            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            defaultBackgroundColour = colourProvider.Background3;
            selectedBackgroundColour = colourProvider.Background1;

            defaultIconColour = defaultBackgroundColour.Darken(0.5f);
            selectedIconColour = selectedBackgroundColour.Lighten(0.5f);

            Add(icon = (CreateIcon?.Invoke() ?? new Circle()).With(b =>
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

            Selected.BindValueChanged(selected =>
            {
                updateSelectionState();
                if (selected.NewValue)
                    action?.Invoke();
            }, true);

            Selected.BindDisabledChanged(disabled => Enabled.Value = !disabled, true);
            updateSelectionState();
        }

        /// <summary>
        /// Selects this <see cref="EditorRadioButton"/>.
        /// </summary>
        public void Select() => Selected.Value = true;

        /// <summary>
        /// Deselects this <see cref="EditorRadioButton"/>.
        /// </summary>
        public void Deselect() => Selected.Value = false;

        private void updateSelectionState()
        {
            if (!IsLoaded)
                return;

            BackgroundColour = Selected.Value ? selectedBackgroundColour : defaultBackgroundColour;
            icon.Colour = Selected.Value ? selectedIconColour : defaultIconColour;
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.CentreLeft,
            Anchor = Anchor.CentreLeft,
            X = 40f
        };

        public LocalisableString TooltipText { get; set; }
    }
}
