// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Cursor;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestCaseGameplayCursor : OsuTestCase, IProvideCursor
    {
        private GameplayCursorContainer cursorContainer;

        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(CursorTrail) };

        public CursorContainer Cursor => cursorContainer;

        public bool ProvidingUserCursor => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(cursorContainer = new OsuCursorContainer { RelativeSizeAxes = Axes.Both });
        }
    }
}
