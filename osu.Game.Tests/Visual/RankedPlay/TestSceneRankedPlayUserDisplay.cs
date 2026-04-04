// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayUserDisplay : MultiplayerTestScene
    {
        private readonly BindableInt health = new BindableInt
        {
            MaxValue = 1_000_000,
            MinValue = 0,
            Value = 1_000_000,
        };

        public TestSceneRankedPlayUserDisplay()
        {
            AddSliderStep("health", 0, 1_000_000, 1_000_000, value => health.Value = value);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("add display", () => Child = new RankedPlayUserDisplay(1001, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(256, 72),
                Health = { BindTarget = health }
            });
        }

        [Test]
        public void TesUserDisplay()
        {
            AddStep("blue color scheme", () => Child = new RankedPlayUserDisplay(1001, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(256, 72),
                Health = { BindTarget = health }
            });

            AddStep("red color scheme", () => Child = new RankedPlayUserDisplay(1001, Anchor.BottomLeft, RankedPlayColourScheme.Red)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(256, 72),
                Health = { BindTarget = health }
            });
        }

        [Test]
        public void TestBeatmapState()
        {
            float progress = 0;

            AddStep("set unavailable", () => MultiplayerClient.ChangeBeatmapAvailability(BeatmapAvailability.NotDownloaded()));
            AddStep("set downloading", () => MultiplayerClient.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(progress = 0)));
            AddUntilStep("increment progress", () =>
            {
                progress += RNG.NextSingle(0.1f);
                MultiplayerClient.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(progress));
                return progress >= 1;
            });
            AddStep("set to importing", () => MultiplayerClient.ChangeBeatmapAvailability(BeatmapAvailability.Importing()));
            AddStep("set to available", () => MultiplayerClient.ChangeBeatmapAvailability(BeatmapAvailability.LocallyAvailable()));
        }
    }
}
