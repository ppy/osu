// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Screens.Select.Options;

namespace osu.Game.Tests.Visual.SongSelect
{
    [Description("bottom beatmap details")]
    public partial class TestSceneBeatmapOptionsOverlay : OsuTestScene
    {
        public TestSceneBeatmapOptionsOverlay()
        {
            var overlay = new BeatmapOptionsOverlay();

            var colours = new OsuColour();

            overlay.AddButton(@"Manage", @"collections", FontAwesome.Solid.Book, colours.Green, null);
            overlay.AddButton(@"Delete", @"all difficulties", FontAwesome.Solid.Trash, colours.Pink, null);
            overlay.AddButton(@"Remove", @"from unplayed", FontAwesome.Regular.TimesCircle, colours.Purple, null);
            overlay.AddButton(@"Clear", @"local scores", FontAwesome.Solid.Eraser, colours.Purple, null);
            overlay.AddButton(@"Edit", @"beatmap", FontAwesome.Solid.PencilAlt, colours.Yellow, null);

            Add(overlay);

            AddStep(@"Toggle", overlay.ToggleVisibility);
        }
    }
}
