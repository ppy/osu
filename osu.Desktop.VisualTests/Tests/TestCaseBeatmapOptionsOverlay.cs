// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Testing;
using osu.Game.Graphics;
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

            overlay.AddButton(@"Remove", @"from unplayed", FontAwesome.fa_times_circle_o, Color4.Purple, null, Key.Number1);
            overlay.AddButton(@"Clear", @"local scores", FontAwesome.fa_eraser, Color4.Purple, null, Key.Number2);
            overlay.AddButton(@"Edit", @"Beatmap", FontAwesome.fa_pencil, Color4.Yellow, null, Key.Number3);
            overlay.AddButton(@"Delete", @"Beatmap", FontAwesome.fa_trash, Color4.Pink, null, Key.Number4, float.MaxValue);

            Add(overlay);

            AddStep(@"Toggle", overlay.ToggleVisibility);
        }
    }
}
