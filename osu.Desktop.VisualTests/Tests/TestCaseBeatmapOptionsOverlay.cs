// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens.Testing;
using osu.Game.Screens.Select.Options;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseBeatmapOptionsOverlay : TestCase
    {
        public override string Description => @"Beatmap options in song select";

        public override void Reset()
        {
            base.Reset();

            var overlay = new BeatmapOptionsOverlay();

            Add(overlay);

            AddButton(@"Toggle", overlay.ToggleVisibility);
        }
    }
}
