// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// Contains dependencies for testing online-play rooms.
    /// </summary>
    public class RoomTestDependencies : IReadOnlyDependencyContainer, IOnlinePlayTestDependencies
    {
        public Bindable<Room> SelectedRoom { get; }
        public IRoomManager RoomManager { get; }
        public Bindable<FilterCriteria> Filter { get; }
        public OngoingOperationTracker OngoingOperationTracker { get; }
        public OnlinePlayBeatmapAvailabilityTracker AvailabilityTracker { get; }

        /// <summary>
        /// All cached dependencies which are also <see cref="Drawable"/> components.
        /// </summary>
        public IReadOnlyList<Drawable> DrawableComponents => drawableComponents;

        private readonly List<Drawable> drawableComponents = new List<Drawable>();
        private readonly DependencyContainer dependencies;

        public RoomTestDependencies()
        {
            SelectedRoom = new Bindable<Room>();
            RoomManager = CreateRoomManager();
            Filter = new Bindable<FilterCriteria>(new FilterCriteria());
            OngoingOperationTracker = new OngoingOperationTracker();
            AvailabilityTracker = new OnlinePlayBeatmapAvailabilityTracker();

            dependencies = new DependencyContainer(new CachedModelDependencyContainer<Room>(null) { Model = { BindTarget = SelectedRoom } });

            CacheAs(SelectedRoom);
            CacheAs(RoomManager);
            CacheAs(Filter);
            CacheAs(OngoingOperationTracker);
            CacheAs(AvailabilityTracker);
        }

        public object Get(Type type)
            => dependencies.Get(type);

        public object Get(Type type, CacheInfo info)
            => dependencies.Get(type, info);

        public void Inject<T>(T instance)
            where T : class
            => dependencies.Inject(instance);

        protected void Cache(object instance)
        {
            dependencies.Cache(instance);
            if (instance is Drawable drawable)
                drawableComponents.Add(drawable);
        }

        protected void CacheAs<T>(T instance)
            where T : class
        {
            dependencies.CacheAs(instance);
            if (instance is Drawable drawable)
                drawableComponents.Add(drawable);
        }

        protected virtual IRoomManager CreateRoomManager() => new BasicTestRoomManager();
    }
}
