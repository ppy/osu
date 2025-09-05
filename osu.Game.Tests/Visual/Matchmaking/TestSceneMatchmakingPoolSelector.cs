// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingPoolSelector : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add selector", () => Child = new MatchmakingPoolSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AvailablePools =
                {
                    Value =
                    [
                        new MatchmakingPool { Id = 0, RulesetId = 0 },
                        new MatchmakingPool { Id = 1, RulesetId = 1 },
                        new MatchmakingPool { Id = 2, RulesetId = 2 },
                        new MatchmakingPool { Id = 3, RulesetId = 3, Variant = 4 },
                        new MatchmakingPool { Id = 4, RulesetId = 3, Variant = 7 },
                    ]
                }
            });
        }
    }
}
