// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingChatDisplay : ScreenTestScene
    {
        private MatchmakingChatDisplay? chat;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add chat", () =>
            {
                chat?.Expire();

                ScreenFooter.Add(chat = new MatchmakingChatDisplay(new Room())
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Size = new Vector2(700, 130),
                    Margin = new MarginPadding { Bottom = 10, Right = WaveOverlayContainer.WIDTH_PADDING - OsuScreen.HORIZONTAL_OVERFLOW_PADDING },
                    Alpha = 0
                });
            });

            AddStep("show footer", () => ScreenFooter.Show());
        }

        [Test]
        public void TestAppearDisappear()
        {
            AddStep("appear", () => chat!.Appear());
            AddWaitStep("wait for animation", 3);

            AddStep("disappear", () => chat!.Disappear());
            AddWaitStep("wait for animation", 3);
        }
    }
}
