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
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class DrawableTernaryButton : OsuButton, IHasTooltip, IHasCurrentValue<TernaryState>
    {
        public Bindable<TernaryState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<TernaryState> current = new BindableWithCurrent<TernaryState>();

        public required LocalisableString Description
        {
            get => Text;
            set => Text = value;
        }

        public LocalisableString TooltipText { get; set; }

        /// <summary>
        /// A function which creates a drawable icon to represent this item. If null, a sane default should be used.
        /// </summary>
        public Func<Drawable>? CreateIcon { get; init; }

        private Color4 defaultBackgroundColour;
        private Color4 defaultIconColour;
        private Color4 selectedBackgroundColour;
        private Color4 selectedIconColour;

        protected Drawable Icon { get; private set; } = null!;

        public DrawableTernaryButton()
        {
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            defaultBackgroundColour = colourProvider.Background3;
            selectedBackgroundColour = colourProvider.Background1;

            defaultIconColour = defaultBackgroundColour.Darken(0.5f);
            selectedIconColour = selectedBackgroundColour.Lighten(0.5f);

            Add(Icon = (CreateIcon?.Invoke() ?? new Circle()).With(b =>
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

            current.BindValueChanged(_ => updateSelectionState(), true);

            Action = onAction;
        }

        private void onAction()
        {
            if (!Enabled.Value)
                return;

            Toggle();
        }

        public void Toggle()
        {
            switch (Current.Value)
            {
                case TernaryState.False:
                case TernaryState.Indeterminate:
                    Current.Value = TernaryState.True;
                    break;

                case TernaryState.True:
                    Current.Value = TernaryState.False;
                    break;
            }
        }

        private void updateSelectionState()
        {
            if (!IsLoaded)
                return;

            switch (Current.Value)
            {
                case TernaryState.Indeterminate:
                    Icon.Colour = selectedIconColour.Darken(0.5f);
                    BackgroundColour = selectedBackgroundColour.Darken(0.5f);
                    break;

                case TernaryState.False:
                    Icon.Colour = defaultIconColour;
                    BackgroundColour = defaultBackgroundColour;
                    break;

                case TernaryState.True:
                    Icon.Colour = selectedIconColour;
                    BackgroundColour = selectedBackgroundColour;
                    break;
            }
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
