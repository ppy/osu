﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Overlays;
using osu.Game.Overlays.Comments;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneTotalCommentsCounter : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneTotalCommentsCounter()
        {
            var count = new BindableInt();

            Add(new TotalCommentsCounter
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
