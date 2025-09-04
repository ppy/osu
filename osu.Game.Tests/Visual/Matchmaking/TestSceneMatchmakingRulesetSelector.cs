// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Matchmaking;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingRulesetSelector : OsuTestScene
    {
        public TestSceneMatchmakingRulesetSelector()
        {
            Child = new MatchmakingRulesetSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }
    }
}
