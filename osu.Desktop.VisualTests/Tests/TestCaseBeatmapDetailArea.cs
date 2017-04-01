// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Select;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseBeatmapDetailArea : TestCase
    {
        public override string Description => @"Beatmap details in song select";

        public override void Reset()
        {
            base.Reset();

            Add(new BeatmapDetailArea
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(550f, 450f),
            });
        }
    }
}