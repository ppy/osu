﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays
{
    public class OverlayScrollContainer : BasicScrollContainer
    {
        public OverlayScrollContainer()
        {
            RelativeSizeAxes = Axes.Both;
            ScrollbarVisible = false;
        }
    }
}
