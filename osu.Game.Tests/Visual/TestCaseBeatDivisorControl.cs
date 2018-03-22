// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Compose;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseBeatDivisorControl : OsuTestCase
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
