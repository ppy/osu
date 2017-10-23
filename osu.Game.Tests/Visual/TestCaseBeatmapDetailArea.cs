﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Select;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    internal class TestCaseBeatmapDetailArea : OsuTestCase
    {
        public override string Description => @"Beatmap details in song select";

        public TestCaseBeatmapDetailArea()
        {
            Add(new BeatmapDetailArea
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(550f, 450f),
            });
        }
    }
}