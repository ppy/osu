// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Options;

namespace osu.Desktop.VisualTests
{
    class TestCaseBeatmapOptionsOverlay : TestCase
    {
        public override string Name => @"Beatmap Options";
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
