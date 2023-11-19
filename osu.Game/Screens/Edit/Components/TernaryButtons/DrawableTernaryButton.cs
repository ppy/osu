// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class DrawableTernaryButton : OsuButton
    {
        private Color4 defaultBackgroundColour;
        private Color4 defaultIconColour;
        private Color4 selectedBackgroundColour;
        private Color4 selectedIconColour;

        private Drawable icon = null!;

        public readonly TernaryButton Button;

        public DrawableTernaryButton(TernaryButton button)
        {
            Button = button;

            Text = button.Description;

            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            defaultBackgroundColour = colourProvider.Background3;
            selectedBackgroundColour = colourProvider.Background1;

            defaultIconColour = defaultBackgroundColour.Darken(0.5f);
            selectedIconColour = selectedBackgroundColour.Lighten(0.5f);

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

            Button.Bindable.BindValueChanged(_ => updateSelectionState(), true);

            Action = onAction;
        }

        private void onAction()
        {
            Button.Toggle();
        }

        private void updateSelectionState()
        {
            if (!IsLoaded)
                return;

            switch (Button.Bindable.Value)
            {
                case TernaryState.Indeterminate:
                    icon.Colour = selectedIconColour.Darken(0.5f);
                    BackgroundColour = selectedBackgroundColour.Darken(0.5f);
                    break;

                case TernaryState.False:
                    icon.Colour = defaultIconColour;
                    BackgroundColour = defaultBackgroundColour;
                    break;

                case TernaryState.True:
                    icon.Colour = selectedIconColour;
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
