// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Tests.Visual.RealtimeMultiplayer
{
    public class TestRealtimeRoomContainer : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        [Cached(typeof(StatefulMultiplayerClient))]
        public readonly TestRealtimeMultiplayerClient Client;

        [Cached(typeof(IRoomManager))]
        public readonly TestRealtimeRoomManager RoomManager;

        [Cached]
        public readonly Bindable<FilterCriteria> Filter = new Bindable<FilterCriteria>(new FilterCriteria());

        public TestRealtimeRoomContainer()
        {
            RelativeSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                Client = new TestRealtimeMultiplayerClient(),
                RoomManager = new TestRealtimeRoomManager(),
                content = new Container { RelativeSizeAxes = Axes.Both }
            });
        }
    }
}
