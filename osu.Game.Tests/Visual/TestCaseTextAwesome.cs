﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseTextAwesome : OsuTestCase
    {
        public override string Description => @"Tests display of icons";

        public TestCaseTextAwesome()
        {
            FillFlowContainer flow;

            Add(flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            int i = 50;
            foreach (FontAwesome fa in Enum.GetValues(typeof(FontAwesome)))
            {
                flow.Add(new SpriteIcon
                {
                    Icon = fa,
                    Size = new Vector2(60),
                    Colour = new Color4(
                        Math.Max(0.5f, RNG.NextSingle()),
                        Math.Max(0.5f, RNG.NextSingle()),
                        Math.Max(0.5f, RNG.NextSingle()),
                        1)
                });

                if (i-- == 0) break;
            }
        }
    }
}
