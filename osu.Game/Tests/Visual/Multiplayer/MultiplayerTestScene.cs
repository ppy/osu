// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

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
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            RoomManager.Schedule(() => RoomManager.PartRoom());

            if (joinRoom)
                RoomManager.Schedule(() => RoomManager.CreateRoom(Room));
        });
    }
}
