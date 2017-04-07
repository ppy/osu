// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseSongProgress : TestCase
    {
        public override string Description => @"With fake data";

        private SongProgress progress;

        public override void Reset()
        {
            base.Reset();

            Add(progress = new SongProgress
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Length = 100,
                OnSeek = time => progress.CurrentTime = time,
            });

            AddStep("Toggle Bar", progress.ToggleVisibility);
            AddStep("New Values", displayNewValues);

            displayNewValues();
        }

        private void displayNewValues()
        {
            List<int> newValues = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                newValues.Add(RNG.Next(0, 6));
            }

            progress.Values = newValues.ToArray();
            progress.CurrentTime = RNG.Next(0, 100);
        }
    }
}
