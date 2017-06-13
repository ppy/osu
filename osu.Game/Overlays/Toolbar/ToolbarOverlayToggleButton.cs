﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarOverlayToggleButton : ToolbarButton
    {
        private readonly Box stateBackground;

        private OverlayContainer stateContainer;

        public OverlayContainer StateContainer
        {
            get { return stateContainer; }
            set
            {
                stateContainer = value;
                Action = stateContainer.ToggleVisibility;
                stateContainer.StateChanged += stateChanged;
            }
        }

        public ToolbarOverlayToggleButton()
        {
            Add(stateBackground = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(150).Opacity(180),
                BlendingMode = BlendingMode.Additive,
                Depth = 2,
                Alpha = 0,
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (stateContainer != null)
                stateContainer.StateChanged -= stateChanged;
        }

        private void stateChanged(VisibilityContainer c, Visibility state)
        {
            switch (state)
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
