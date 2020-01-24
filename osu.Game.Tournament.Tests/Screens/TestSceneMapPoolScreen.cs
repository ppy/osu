// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Tournament.Screens.MapPool;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneMapPoolScreen : LadderTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MapPoolScreen)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new MapPoolScreen { Width = 0.7f });
        }
    }
}
