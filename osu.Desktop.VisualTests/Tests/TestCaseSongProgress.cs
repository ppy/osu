// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Screens.Testing;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseSongProgress : TestCase
    {
        public override string Description => @"With (half)real data";

        private SongProgress progress;

        public override void Reset()
        {
            base.Reset();

            Add(progress = new SongProgress
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X
            });

            AddButton("Toggle Bar", progress.ToggleVisibility);
            AddButton("New Values", displayNewValues);

            displayNewValues();
        }

        private void displayNewValues()
        {
            List<int> newValues = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                newValues.Add(RNG.Next(0, 11));
            }

            progress.DisplayValues(newValues.ToArray());
        }
    }
}
