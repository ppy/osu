// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Overlays.Comments;
using osu.Framework.Utils;
using osu.Game.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneTotalCommentsCounter : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(TotalCommentsCounter),
        };

        public TestSceneTotalCommentsCounter()
        {
            var count = new BindableInt();

            Add(new TotalCommentsCounter(OverlayColourScheme.Blue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = { BindTarget = count }
            });

            AddStep(@"Set 100", () => count.Value = 100);
            AddStep(@"Set 0", () => count.Value = 0);
            AddStep(@"Set random", () => count.Value = RNG.Next(0, int.MaxValue));
        }
    }
}
