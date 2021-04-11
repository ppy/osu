// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestMultiplayerRoomContainer : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        [Cached(typeof(StatefulMultiplayerClient))]
        public readonly TestMultiplayerClient Client;

        [Cached(typeof(IRoomManager))]
        public readonly TestMultiplayerRoomManager RoomManager;

        [Cached]
        public readonly Bindable<FilterCriteria> Filter = new Bindable<FilterCriteria>(new FilterCriteria());

        [Cached]
        public readonly OngoingOperationTracker OngoingOperationTracker;

        public TestMultiplayerRoomContainer()
        {
            RelativeSizeAxes = Axes.Both;

            RoomManager = new TestMultiplayerRoomManager();
            Client = new TestMultiplayerClient(RoomManager);
            OngoingOperationTracker = new OngoingOperationTracker();

            AddRangeInternal(new Drawable[]
            {
                Client,
                RoomManager,
                OngoingOperationTracker,
                content = new Container { RelativeSizeAxes = Axes.Both }
            });
        }
    }
}
