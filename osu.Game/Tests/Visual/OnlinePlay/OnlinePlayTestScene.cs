// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// A base test scene for all online play components and screens.
    /// </summary>
    public abstract partial class OnlinePlayTestScene : ScreenTestScene, IOnlinePlayTestSceneDependencies
    {
        public Bindable<Room> SelectedRoom => OnlinePlayDependencies.SelectedRoom;
        public IRoomManager RoomManager => OnlinePlayDependencies.RoomManager;
        public OngoingOperationTracker OngoingOperationTracker => OnlinePlayDependencies.OngoingOperationTracker;
        public OnlinePlayBeatmapAvailabilityTracker AvailabilityTracker => OnlinePlayDependencies.AvailabilityTracker;
        public TestUserLookupCache UserLookupCache => OnlinePlayDependencies.UserLookupCache;
        public BeatmapLookupCache BeatmapLookupCache => OnlinePlayDependencies.BeatmapLookupCache;

        /// <summary>
        /// All dependencies required for online play components and screens.
        /// </summary>
        protected OnlinePlayTestSceneDependencies OnlinePlayDependencies => dependencies.OnlinePlayDependencies!;

        protected override Container<Drawable> Content => content;

        private readonly Container content;
        private readonly Container drawableDependenciesContainer;
        private DelegatedDependencyContainer dependencies = null!;

        protected OnlinePlayTestScene()
        {
            base.Content.AddRange(new Drawable[]
            {
                drawableDependenciesContainer = new Container { RelativeSizeAxes = Axes.Both },
                content = new Container { RelativeSizeAxes = Axes.Both },
            });
        }

        protected sealed override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DelegatedDependencyContainer(base.CreateChildDependencies(parent));

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup dependencies", () =>
            {
                // Reset the room dependencies to a fresh state.
                dependencies.OnlinePlayDependencies = CreateOnlinePlayDependencies();
                drawableDependenciesContainer.Clear();
                drawableDependenciesContainer.AddRange(dependencies.OnlinePlayDependencies.DrawableComponents);

                var handler = OnlinePlayDependencies.RequestsHandler;

                // Resolving the BeatmapManager in the test scene will inject the game-wide BeatmapManager, while many test scenes cache their own BeatmapManager instead.
                // To get around this, the BeatmapManager is looked up from the dependencies provided to the children of the test scene instead.
                var beatmapManager = dependencies.Get<BeatmapManager>();

                ((DummyAPIAccess)API).HandleRequest = request =>
                {
                    try
                    {
                        return handler.HandleRequest(request, API.LocalUser.Value, beatmapManager);
                    }
                    catch (ObjectDisposedException)
                    {
                        // These requests can be fired asynchronously, but potentially arrive after game components
                        // have been disposed (ie. realm in BeatmapManager).
                        // This only happens in tests and it's easiest to ignore them for now.
                        Logger.Log($"Handled {nameof(ObjectDisposedException)} in test request handling");
                        return true;
                    }
                };
            });
        }

        /// <summary>
        /// Creates the room dependencies. Called every <see cref="SetUpSteps"/>.
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
            public OnlinePlayTestSceneDependencies? OnlinePlayDependencies { get; set; }

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
                where T : class, IDependencyInjectionCandidate
                => injectableDependencies.Inject(instance);
        }
    }
}
