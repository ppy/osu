// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public abstract class MultiplayerTestScene : RoomTestScene
    {
        [Cached(typeof(StatefulMultiplayerClient))]
        public TestMultiplayerClient Client { get; }

        [Cached(typeof(IRoomManager))]
        public TestMultiplayerRoomManager RoomManager { get; }

        [Cached]
        public Bindable<FilterCriteria> Filter { get; }

        [Cached]
        public OngoingOperationTracker OngoingOperationTracker { get; }

        protected override Container<Drawable> Content => content;
        private readonly TestMultiplayerRoomContainer content;

        private readonly bool joinRoom;

        protected MultiplayerTestScene(bool joinRoom = true)
        {
            this.joinRoom = joinRoom;
            base.Content.Add(content = new TestMultiplayerRoomContainer { RelativeSizeAxes = Axes.Both });

            Client = content.Client;
            RoomManager = content.RoomManager;
            Filter = content.Filter;
            OngoingOperationTracker = content.OngoingOperationTracker;
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            RoomManager.Schedule(() => RoomManager.PartRoom());

            if (joinRoom)
            {
                Room.Name.Value = "test name";
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(Ruleset.Value).BeatmapInfo },
                    Ruleset = { Value = Ruleset.Value }
                });

                RoomManager.Schedule(() => RoomManager.CreateRoom(Room));
            }
        });

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            if (joinRoom)
                AddUntilStep("wait for room join", () => Client.Room != null);
        }
    }
}
