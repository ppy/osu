// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayMatchPanel : MultiplayerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Test]
        public void TestLeftWin()
        {
            AddStep("add panel", () => Child = new DelayedLoadWrapper(new RankedPlayMatchPanel(new RankedPlayRoomState
            {
                Users =
                {
                    { 1, new RankedPlayUserInfo { Rating = 0, Life = 800_000, RoundsWon = 3 } },
                    { 2, new RankedPlayUserInfo { Rating = 0, Life = 200_000, RoundsWon = 1 } }
                }
            }), 0)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 280
            });
        }

        [Test]
        public void TestRightWin()
        {
            AddStep("add panel", () => Child = new DelayedLoadWrapper(new RankedPlayMatchPanel(new RankedPlayRoomState
            {
                Users =
                {
                    { 1, new RankedPlayUserInfo { Rating = 0, Life = 200_000, RoundsWon = 3 } },
                    { 2, new RankedPlayUserInfo { Rating = 0, Life = 800_000, RoundsWon = 1 } }
                }
            }), 0)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 280
            });
        }

        [Test]
        public void TestDraw()
        {
            AddStep("add panel", () => Child = new DelayedLoadWrapper(new RankedPlayMatchPanel(new RankedPlayRoomState
            {
                Users =
                {
                    { 1, new RankedPlayUserInfo { Rating = 0, Life = 200_000, RoundsWon = 3 } },
                    { 2, new RankedPlayUserInfo { Rating = 0, Life = 200_000, RoundsWon = 1 } }
                }
            }), 0)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 280
            });
        }
    }
}
