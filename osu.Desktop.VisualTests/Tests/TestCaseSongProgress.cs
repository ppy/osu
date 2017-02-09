// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Game.Screens.Play;
using osu.Framework.Graphics;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Sprites;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Colour;
using System.Collections.Generic;

namespace osu.Desktop.VisualTests
{
    public class TestCaseSongProgress : TestCase
    {
        public override string Name => @"Song Progress";
        public override string Description => @"With real data";

        private SongProgress progress;

        public override void Reset()
        {
            base.Reset();

            Add(new Box
            {
                Colour = Color4.Gray,
                RelativeSizeAxes = Axes.Both
            });
            Add(progress = new SongProgress
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X
            });

            var random = new Random();

            List<int> newValues = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                newValues.Add(random.Next(1, 11));
            }

            progress.DisplayValues(newValues);
        }
    }
}
