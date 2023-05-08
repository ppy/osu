// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Screens.Editors;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneLadderEditorScreen : TournamentTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new LadderEditorScreen()
            });
        }
    }
}
