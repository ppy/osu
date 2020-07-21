// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneLoungeFilterControl : OsuTestScene
    {
        public TestSceneLoungeFilterControl()
        {
            Child = new FilterControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }
    }
}
