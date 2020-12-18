// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.RealtimeMultiplayer;

namespace osu.Game.Tests.Visual.RealtimeMultiplayer
{
    public class RealtimeMultiplayerTestScene : MultiplayerTestScene
    {
        [Cached(typeof(StatefulMultiplayerClient))]
        public TestRealtimeMultiplayerClient Client { get; }

        [Cached(typeof(RealtimeRoomManager))]
        public TestRealtimeRoomManager RoomManager { get; }

        [Cached]
        public Bindable<FilterCriteria> Filter { get; }

        protected override Container<Drawable> Content => content;
        private readonly TestRealtimeRoomContainer content;

        private readonly bool joinRoom;

        public RealtimeMultiplayerTestScene(bool joinRoom = true)
        {
            this.joinRoom = joinRoom;
            base.Content.Add(content = new TestRealtimeRoomContainer { RelativeSizeAxes = Axes.Both });

            Client = content.Client;
            RoomManager = content.RoomManager;
            Filter = content.Filter;
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            RoomManager.Schedule(() => RoomManager.PartRoom());

            if (joinRoom)
            {
                Room.RoomID.Value = 1;
                RoomManager.Schedule(() => RoomManager.JoinRoom(Room, null, null));
            }
        });
    }
}
