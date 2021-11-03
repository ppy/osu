// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// Contains the basic dependencies of online play test scenes.
    /// </summary>
    public class OnlinePlayTestSceneDependencies : IReadOnlyDependencyContainer, IOnlinePlayTestSceneDependencies
    {
        public Bindable<Room> SelectedRoom { get; }
        public IRoomManager RoomManager { get; }
        public OngoingOperationTracker OngoingOperationTracker { get; }
        public OnlinePlayBeatmapAvailabilityTracker AvailabilityTracker { get; }
        public TestRoomRequestsHandler RequestsHandler { get; }

        /// <summary>
        /// All cached dependencies which are also <see cref="Drawable"/> components.
        /// </summary>
        public IReadOnlyList<Drawable> DrawableComponents => drawableComponents;

        private readonly List<Drawable> drawableComponents = new List<Drawable>();
        private readonly DependencyContainer dependencies;

        public OnlinePlayTestSceneDependencies()
        {
            SelectedRoom = new Bindable<Room>();
            RequestsHandler = new TestRoomRequestsHandler();
            OngoingOperationTracker = new OngoingOperationTracker();
            AvailabilityTracker = new OnlinePlayBeatmapAvailabilityTracker();
            RoomManager = CreateRoomManager();

            dependencies = new DependencyContainer(new CachedModelDependencyContainer<Room>(null) { Model = { BindTarget = SelectedRoom } });

            CacheAs(RequestsHandler);
            CacheAs(SelectedRoom);
            CacheAs(RoomManager);
            CacheAs(OngoingOperationTracker);
            CacheAs(AvailabilityTracker);
            CacheAs(new OverlayColourProvider(OverlayColourScheme.Plum));
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

        protected virtual IRoomManager CreateRoomManager() => new TestRoomManager();
    }
}
