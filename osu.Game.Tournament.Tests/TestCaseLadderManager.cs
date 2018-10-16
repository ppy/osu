// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Screens.Ladder;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseLadderManager : LadderTestCase
    {
        [Cached]
        private LadderManager manager;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = manager = new LadderManager(Ladder)
            });
        }

        protected override void SaveChanges()
        {
            Ladder = manager.CreateInfo();
            base.SaveChanges();
        }
    }
}
