// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneRankingsOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankingsOverlay),
            typeof(RankingsHeader),
        };

        public TestSceneRankingsOverlay()
        {
            var overlay = new RankingsOverlay();

            Add(overlay);

            AddStep("Toggle visibility", overlay.ToggleVisibility);
        }
    }
}
