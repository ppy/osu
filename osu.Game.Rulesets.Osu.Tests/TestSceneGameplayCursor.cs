// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.UI.Cursor;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneGameplayCursor : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(CursorTrail) };

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new OsuCursorContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
            });
        }
    }
}
