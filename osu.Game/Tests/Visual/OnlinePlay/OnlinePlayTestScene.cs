// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// A <see cref="ScreenTestScene"/> providing all the dependencies cached by <see cref="OnlinePlayScreen"/> for testing <see cref="OnlinePlaySubScreen"/>s.
    /// </summary>
    public abstract class OnlinePlayTestScene : ScreenTestScene, IRoomTestDependencies
    {
        public Bindable<Room> SelectedRoom => RoomDependencies?.SelectedRoom;
        public IRoomManager RoomManager => RoomDependencies?.RoomManager;
        public Bindable<FilterCriteria> Filter => RoomDependencies?.Filter;
        public OngoingOperationTracker OngoingOperationTracker => RoomDependencies?.OngoingOperationTracker;
        public OnlinePlayBeatmapAvailabilityTracker AvailabilityTracker => RoomDependencies?.AvailabilityTracker;

        protected RoomTestDependencies RoomDependencies => delegatedDependencies?.RoomDependencies;
        private DelegatedRoomDependencyContainer delegatedDependencies;

        protected override Container<Drawable> Content => content;
        private readonly Container content;
        private readonly Container drawableDependenciesContainer;

        protected OnlinePlayTestScene()
        {
            base.Content.AddRange(new Drawable[]
            {
                drawableDependenciesContainer = new Container { RelativeSizeAxes = Axes.Both },
                content = new Container { RelativeSizeAxes = Axes.Both },
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            delegatedDependencies = new DelegatedRoomDependencyContainer(base.CreateChildDependencies(parent));
            return delegatedDependencies;
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            // Reset the room dependencies to a fresh state.
            drawableDependenciesContainer.Clear();
            delegatedDependencies.RoomDependencies = CreateRoomDependencies();
            drawableDependenciesContainer.AddRange(RoomDependencies.DrawableComponents);
        });

        /// <summary>
        /// Creates the room dependencies. Called every <see cref="Setup"/>.
        /// </summary>
        /// <remarks>
        /// Any custom dependencies required for online-play sub-classes should be added here.
        /// </remarks>
        protected virtual RoomTestDependencies CreateRoomDependencies() => new RoomTestDependencies();

        /// <summary>
        /// A <see cref="IReadOnlyDependencyContainer"/> providing a mutable lookup source for room dependencies.
        /// </summary>
        private class DelegatedRoomDependencyContainer : IReadOnlyDependencyContainer
        {
            /// <summary>
            /// The room's dependencies.
            /// </summary>
            public RoomTestDependencies RoomDependencies { get; set; }

            private readonly IReadOnlyDependencyContainer parent;
            private readonly DependencyContainer injectableDependencies;

            /// <summary>
            /// Creates a new <see cref="DelegatedRoomDependencyContainer"/>.
            /// </summary>
            /// <param name="parent">The fallback <see cref="IReadOnlyDependencyContainer"/> to use when <see cref="RoomDependencies"/> cannot satisfy a dependency.</param>
            public DelegatedRoomDependencyContainer(IReadOnlyDependencyContainer parent)
            {
                this.parent = parent;
                injectableDependencies = new DependencyContainer(this);
            }

            public object Get(Type type)
                => RoomDependencies?.Get(type) ?? parent.Get(type);

            public object Get(Type type, CacheInfo info)
                => RoomDependencies?.Get(type, info) ?? parent.Get(type, info);

            public void Inject<T>(T instance)
                where T : class
                => injectableDependencies.Inject(instance);
        }
    }
}
