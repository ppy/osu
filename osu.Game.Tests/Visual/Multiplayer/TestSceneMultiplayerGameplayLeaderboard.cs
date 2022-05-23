// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerGameplayLeaderboard : MultiplayerGameplayLeaderboardTestScene
    {
        protected override MultiplayerGameplayLeaderboard CreateLeaderboard(OsuScoreProcessor scoreProcessor)
        {
            return new MultiplayerGameplayLeaderboard(Ruleset.Value, scoreProcessor, MultiplayerUsers.ToArray())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }
    }
}
