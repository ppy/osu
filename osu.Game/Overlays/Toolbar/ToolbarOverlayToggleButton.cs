﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarOverlayToggleButton : ToolbarButton
    {
        private readonly Box stateBackground;

        private OverlayContainer stateContainer;

        private readonly Bindable<Visibility> overlayState = new Bindable<Visibility>();

        public OverlayContainer StateContainer
        {
            get => stateContainer;
            set
            {
                stateContainer = value;

                overlayState.UnbindBindings();

                if (stateContainer != null)
                {
                    Action = stateContainer.ToggleVisibility;
                    overlayState.BindTo(stateContainer.State);
                }

                if (stateContainer is INamedOverlayComponent named)
                {
                    TooltipMain = named.Title;
                    TooltipSub = named.Description;
                    SetIcon(named.IconTexture);
                }
            }
        }

        public ToolbarOverlayToggleButton()
        {
            Add(stateBackground = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(150).Opacity(180),
                Blending = BlendingParameters.Additive,
                Depth = 2,
                Alpha = 0,
            });

            overlayState.ValueChanged += stateChanged;
        }

        private void stateChanged(ValueChangedEvent<Visibility> state)
        {
            switch (state.NewValue)
            {
                case Visibility.Hidden:
                    stateBackground.FadeOut(200);
                    break;

                case Visibility.Visible:
                    stateBackground.FadeIn(200);
                    break;
            }
        }
    }
}
