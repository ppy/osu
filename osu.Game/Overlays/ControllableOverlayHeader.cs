﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class ControllableOverlayHeader : OverlayHeader
    {
        private readonly Box controlBackground;

        protected Color4 ControlBackgroundColour
        {
            set => controlBackground.Colour = value;
        }

        protected ControllableOverlayHeader()
        {
            HeaderInfo.Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    controlBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    },
                    CreateControl().With(control => control.Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN })
                }
            });
        }

        protected abstract TabControl<string> CreateControl();
    }
}
