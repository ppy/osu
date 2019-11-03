// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Editor
{
    public class TestSceneBeatDivisorControl : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(BindableBeatDivisor) };

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new BeatDivisorControl(new BindableBeatDivisor())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(90, 90)
            };
        }
    }
}
