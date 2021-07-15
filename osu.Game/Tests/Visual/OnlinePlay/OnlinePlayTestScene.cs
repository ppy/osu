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
    /// A base test scene for all online play components and screens.
    /// </summary>
    public abstract class OnlinePlayTestScene : ScreenTestScene, IOnlinePlayTestSceneDependencies
    {
        public Bindable<Room> SelectedRoom => OnlinePlayDependencies?.SelectedRoom;
        public IRoomManager RoomManager => OnlinePlayDependencies?.RoomManager;
        public Bindable<FilterCriteria> Filter => OnlinePlayDependencies?.Filter;
        public OngoingOperationTracker OngoingOperationTracker => OnlinePlayDependencies?.OngoingOperationTracker;
        public OnlinePlayBeatmapAvailabilityTracker AvailabilityTracker => OnlinePlayDependencies?.AvailabilityTracker;

        /// <summary>
        /// All dependencies required for online play components and screens.
        /// </summary>
        protected OnlinePlayTestSceneDependencies OnlinePlayDependencies => dependencies?.OnlinePlayDependencies;

        private DelegatedDependencyContainer dependencies;

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
            dependencies = new DelegatedDependencyContainer(base.CreateChildDependencies(parent));
            return dependencies;
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            // Reset the room dependencies to a fresh state.
            drawableDependenciesContainer.Clear();
            dependencies.OnlinePlayDependencies = CreateOnlinePlayDependencies();
            drawableDependenciesContainer.AddRange(OnlinePlayDependencies.DrawableComponents);
        });

        /// <summary>
        /// Creates the room dependencies. Called every <see cref="Setup"/>.
        /// </summary>
        /// <remarks>
        /// Any custom dependencies required for online play sub-classes should be added here.
        /// </remarks>
        protected virtual OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new OnlinePlayTestSceneDependencies();

        /// <summary>
        /// A <see cref="IReadOnlyDependencyContainer"/> providing a mutable lookup source for online play dependencies.
        /// </summary>
        private class DelegatedDependencyContainer : IReadOnlyDependencyContainer
        {
            /// <summary>
            /// The online play dependencies.
            /// </summary>
            public OnlinePlayTestSceneDependencies OnlinePlayDependencies { get; set; }

            private readonly IReadOnlyDependencyContainer parent;
            private readonly DependencyContainer injectableDependencies;

            /// <summary>
            /// Creates a new <see cref="DelegatedDependencyContainer"/>.
            /// </summary>
            /// <param name="parent">The fallback <see cref="IReadOnlyDependencyContainer"/> to use when <see cref="OnlinePlayDependencies"/> cannot satisfy a dependency.</param>
            public DelegatedDependencyContainer(IReadOnlyDependencyContainer parent)
            {
                this.parent = parent;
                injectableDependencies = new DependencyContainer(this);
            }

            public object Get(Type type)
                => OnlinePlayDependencies?.Get(type) ?? parent.Get(type);

            public object Get(Type type, CacheInfo info)
                => OnlinePlayDependencies?.Get(type, info) ?? parent.Get(type, info);

            public void Inject<T>(T instance)
                where T : class
                => injectableDependencies.Inject(instance);
        }
    }
}
