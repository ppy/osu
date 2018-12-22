// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match.Components;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMatchSettingsOverlay : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RoomSettingsOverlay)
        };

        public TestCaseMatchSettingsOverlay()
        {
            Child = new RoomSettingsOverlay(new Room())
            {
                RelativeSizeAxes = Axes.Both,
                State = Visibility.Visible
            };
        }
    }
}
