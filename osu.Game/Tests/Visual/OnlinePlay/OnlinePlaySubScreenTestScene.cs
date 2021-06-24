// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// A <see cref="ScreenTestScene"/> providing all the dependencies cached by <see cref="OnlinePlayScreen"/> for testing <see cref="OnlinePlaySubScreen"/>s.
    /// </summary>
    public abstract class OnlinePlaySubScreenTestScene : ScreenTestScene
    {
        /// <summary>
        /// The cached <see cref="SelectedRoom"/>.
        /// </summary>
        protected Bindable<Room> SelectedRoom { get; private set; }

        /// <summary>
        /// The cached <see cref="IRoomManager"/>
        /// </summary>
        protected IRoomManager RoomManager { get; private set; }

        protected Bindable<FilterCriteria> Filter { get; private set; }

        protected OngoingOperationTracker OngoingOperationTracker { get; private set; }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create dependencies", () => LoadScreen(new DependenciesScreen(CreateScreenDependencies)));
        }

        /// <summary>
        /// Creates dependencies for any <see cref="OsuScreen"/> pushed via <see cref="ScreenTestScene.LoadScreen"/>.
        /// Invoked at the start of every test via <see cref="SetUpSteps"/>.
        /// </summary>
        /// <remarks>
        /// This should be overridden to add any custom dependencies required by subclasses of <see cref="OnlinePlaySubScreen"/>.
        /// </remarks>
        /// <param name="parent">The parent dependency container.</param>
        /// <returns>The resultant dependency container.</returns>
        protected virtual IReadOnlyDependencyContainer CreateScreenDependencies(IReadOnlyDependencyContainer parent)
        {
            SelectedRoom = new Bindable<Room>();
            RoomManager = CreateRoomManager();
            Filter = new Bindable<FilterCriteria>(new FilterCriteria());
            OngoingOperationTracker = new OngoingOperationTracker();

            var dependencies = new DependencyContainer(new CachedModelDependencyContainer<Room>(parent) { Model = { BindTarget = SelectedRoom } });
            dependencies.CacheAs(SelectedRoom);
            dependencies.CacheAs(RoomManager);
            dependencies.CacheAs(Filter);
            dependencies.CacheAs(OngoingOperationTracker);

            return dependencies;
        }

        protected virtual IRoomManager CreateRoomManager() => new TestBasicRoomManager();

        /// <summary>
        /// A dummy screen used for injecting new dependencies into the hierarchy before any screen is pushed via <see cref="ScreenTestScene.LoadScreen"/>.
        /// </summary>
        private class DependenciesScreen : OsuScreen
        {
            private readonly Func<IReadOnlyDependencyContainer, IReadOnlyDependencyContainer> createDependenciesFunc;

            public DependenciesScreen(Func<IReadOnlyDependencyContainer, IReadOnlyDependencyContainer> createDependenciesFunc)
            {
                this.createDependenciesFunc = createDependenciesFunc;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
                => createDependenciesFunc(base.CreateChildDependencies(parent));
        }
    }
}
