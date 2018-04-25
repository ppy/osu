// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Cursor;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestCaseGameplayCursor : OsuTestCase, IProvideCursor
    {
        private GameplayCursor cursor;

        public override IReadOnlyList<Type> RequiredTypes => new [] { typeof(CursorTrail) };

        public CursorContainer Cursor => cursor;

        public bool ProvidingUserCursor => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(cursor = new GameplayCursor { RelativeSizeAxes = Axes.Both });
        }
    }
}
