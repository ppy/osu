﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Options
{
    public class SectionContainer : SearchContainer
    {
        private FillFlowContainer flow;

        protected override Container<Drawable> Content => flow;

        public SectionContainer()
        {
            InternalChildren = new[]
            {
                flow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                },
            };
        }
    }
}
