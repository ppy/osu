// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Game.Graphics;
using osu.Game.Screens.Select.Options;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual
{
    [Description("bottom beatmap details")]
    public class TestCaseBeatmapOptionsOverlay : OsuTestCase
    {
        public TestCaseBeatmapOptionsOverlay()
        {
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
