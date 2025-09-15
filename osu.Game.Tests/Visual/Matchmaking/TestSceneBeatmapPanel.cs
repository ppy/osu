// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapPanel : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add beatmap panel", () =>
            {
                Child = new BeatmapPanel(CreateAPIBeatmap())
                {
                    Size = new Vector2(300, 70),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });
        }
    }
}
